﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Sql.Parser;
using Sql.Parser.Mapper;
using Sql.Parser.Parser;

namespace XAdo.UnitTest
{
   public class Person
   {
      public int Id { get; protected set; }
      public string PersonType { get; set; }
      public string Title { get; set; }
      public string FirstName { get; set; }
      public string MiddleName { get; set; }
      public string LastName { get; set; }
      public DateTime ModifiedAt { get; set; }
      public Address Address { get; set; }
   }

   public class Address
   {
      public int Id { get; protected set; }
      public string Line1 { get; set; }
      public string Line2 { get; set; }
      public string City { get; set; }
      public string PostalCode { get; set; }
      public DateTime ModifiedAt { get; set; }
      public AddressType AddressType { get; set; }
   }

   public class AddressType
   {
      public int Id { get; protected set; }
      public string Name { get; protected set; }
   }

   [TestFixture]
   public class AWTest2
   {

      public const string Query3 = @"
SELECT        
   p.BusinessEntityID as id, -->*
   p.PersonType, -->! 
   p.Title, 
   p.FirstName, -->! 
   p.MiddleName, 
   p.LastName, -->! 
   p.ModifiedDate as ModifiedAt, 
   a.AddressID, --> Address/Id* 
   a.AddressLine1 as Line1, -->!
   a.AddressLine2 as Line2, 
   a.PostalCode, -->! 
   a.ModifiedDate as ModifiedAt,
   at.AddressTypeID, -->AddressType/Id*
   at.Name, -->!
   a.City -->../City!
FROM Person.BusinessEntity AS be 
INNER JOIN Person.Person AS p ON be.BusinessEntityID = p.BusinessEntityID 
INNER JOIN Person.BusinessEntityAddress AS bea ON be.BusinessEntityID = bea.BusinessEntityID 
INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID 
INNER JOIN Person.AddressType AS at ON bea.AddressTypeID = at.AddressTypeID
--$WHERE {where}
--${!order}{!inner}ORDER BY p.BusinessEntityID
--$ORDER BY {order} 
--$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY

";

      public const string Query4 = @"
SELECT        
   p.BusinessEntityID as id, -->*
   p.PersonType, -->! 
   p.Title, 
   p.FirstName, -->! 
   p.MiddleName, 
   p.LastName, -->! 
   p.ModifiedDate as ModifiedAt, 
   a.AddressID, --> Address/Id* 
   a.AddressLine1 as Line1, -->!
   a.AddressLine2 as Line2, 
   a.PostalCode, -->! 
   a.ModifiedDate as ModifiedAt,
   at.AddressTypeID, -->AddressType/Id*
   at.Name, -->!
   a.City -->../City!
FROM Person.BusinessEntity AS be 
INNER JOIN Person.Person AS p ON be.BusinessEntityID = p.BusinessEntityID 
INNER JOIN Person.BusinessEntityAddress AS bea ON be.BusinessEntityID = bea.BusinessEntityID 
INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID 
INNER JOIN Person.AddressType AS at ON bea.AddressTypeID = at.AddressTypeID";


      private const string Template = @"
{?take}{!order}raiserror ('no order specified', 16, 10); 
{?take}SELECT * FROM (
   $SELECT
   {?take},ROW_NUMBER() OVER (ORDER BY {order}) AS __rownum
   $FROM
   $WHERE          // this is comment
   WHERE {where}  
   $GROUP_BY
   $HAVING
   HAVING {having}
   {!take}ORDER BY {order}
   {?take}) AS __paged
{?order}WHERE __rowNum > {skip} AND __rowNum <= {skip}+{take} ORDER BY __rowNum";

      [Test]
      public async void MonkeyTest()
      {
         var selectParser = new SqlSelectParser();
         var tokens = selectParser.Parse(Query3);
         var map = new QueryBuilder(tokens);
         var factory = map.GetBinderExpression<Person>();
         var mapped = map.Map(CreateMap<Person,Address>(p => new Address{Line1 = p.Address.Line1}));
         var f2 = mapped.GetBinderExpression<Address>();
         var ctor = factory.Compile();

         var context = new AdoContext("AW");

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query(Query3, ctor).ToList();
            Type type = persons.First().GetType();
            Debug.WriteLine(type);
            persons = await sn.QueryAsync(Query3, ctor);
            var sw = new Stopwatch();
            sw.Start();
            persons = sn.Query(Query3, ctor).ToList();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }
      [Test]
      public async void MonkeyTest2()
      {
         var q = QueryBuilder<Person>.Parse(Query3);
         var m = q.Map(p => new Address {Line1 = p.Address.Line1});

         var context = new AdoContext("AW");

         var qsql = q.ToString();
         var msql = m.ToString();

         q.GetBinder();

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            q.GetBinder();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query(qsql, q.GetBinder()).ToList();
            persons = await sn.QueryAsync(qsql, q.GetBinder());
            sw = new Stopwatch();
            sw.Start();
            persons = sn.Query(qsql, q.GetBinder()).ToList();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [Test]
      public async void MonkeyTest3()
      {
         var q = QueryBuilder<Person>.Parse(Query4, Template);
         var m = q.Map(p => new Address { Line1 = p.Address.Line1 });

         var context = new AdoContext("AW");

         var qsql = q.ToString();
         var msql = m.ToString();

         q.GetBinder();

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            q.GetBinder();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         qsql = q.Format(new { skip = 10, take = 100 });
         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            q.Format(new { skip = 10, take = 100, order = "p.BusinessEntityID" });
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query(qsql, q.GetBinder()).ToList();
            persons = await sn.QueryAsync(qsql, q.GetBinder());
            sw = new Stopwatch();
            sw.Start();
            persons = sn.Query(qsql, q.GetBinder()).ToList();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      private Expression<Func<TFrom, TTo>> CreateMap<TFrom, TTo>(Expression<Func<TFrom, TTo>> map)
      {
         return map;
      }

   }
}