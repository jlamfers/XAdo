﻿using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using XAdo.Core.Interface;
using XAdo.Sql;
using XAdo.Sql.Core;
using XAdo.Sql.Providers;

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
   public class AWTest
   {

      public const string Query2_old = @"
SELECT        
   p.BusinessEntityID as [/Id*],
   p.PersonType as [!], 
   p.Title, 
   p.FirstName as [!], 
   p.MiddleName, 
   p.LastName as [!],  
   p.ModifiedDate as ModifiedAt, 
   a.AddressID as [/Address/Id*?], 
   a.AddressLine1 as [./Line1!], 
   a.AddressLine2 as [./Line2], 
   a.PostalCode as [!], 
   a.ModifiedDate as [./ModifiedAt],
   at.AddressTypeID as [./AddressType/Id*?],
   at.Name as [!],
   a.City as [../City!]

FROM Person.BusinessEntity AS be 

INNER JOIN Person.Person AS p ON be.BusinessEntityID = p.BusinessEntityID 
INNER JOIN Person.BusinessEntityAddress AS bea ON be.BusinessEntityID = bea.BusinessEntityID 
INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID 
INNER JOIN Person.AddressType AS at ON bea.AddressTypeID = at.AddressTypeID
--$WHERE {where}
--$ORDER BY {order} --?? ORDER BY p.BusinessEntityID -- TODO: default order, default value in general
--$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY

";

      public const string Query3_old = @"
SELECT        
   p.BusinessEntityID, --> /Id*
   p.PersonType, -->! 
   p.Title, 
   p.FirstName, -->! 
   p.MiddleName, 
   p.LastName, -->! 
   p.ModifiedDate, -->ModifiedAt 
   a.AddressID, --> /Address/Id*? 
   a.AddressLine1, -->./Line1! 
   a.AddressLine2, -->./Line2 
   a.PostalCode, -->! 
   a.ModifiedDate, -->./ModifiedAt
   at.AddressTypeID, -->./AddressType/Id*?
   at.Name, -->!
   a.City -->../City!

FROM Person.BusinessEntity AS be 

INNER JOIN Person.Person AS p ON be.BusinessEntityID = p.BusinessEntityID 
INNER JOIN Person.BusinessEntityAddress AS bea ON be.BusinessEntityID = bea.BusinessEntityID 
INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID 
INNER JOIN Person.AddressType AS at ON bea.AddressTypeID = at.AddressTypeID
--$WHERE {where}
--$ORDER BY {order} --?? ORDER BY p.BusinessEntityID -- TODO: default order, default value in general
--$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY

";
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
--$ORDER BY {order} 
--${!order}{!inner}ORDER BY Id
--$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY

";

      [Test]
      public async void MonkeyTest()
      {
         var selectParser = new SqlSelectParser();
         var selectInfo = selectParser.Parse(Query3);
         var factory = selectInfo.BuildFactory<Person>();
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
      public void MonkeyTest2()
      {
         var selectParser = new SqlSelectParser();
         var selectInfo = selectParser.Parse(Query3);
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            selectParser.Parse(Query3);
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
         var factory = selectInfo.BuildFactory<Person>();
         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            selectInfo.BuildFactory<Person>();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
         var ctor = factory.Compile();
         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            factory.Compile();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         var context = new AdoContext(cfg => cfg
            .SetConnectionStringName("AW")
            .BindSingleton<ISqlDialect,SqlServerDialect>()
            .BindSingleton<IQuob<Person>>(b => new Quob<Person>(Query3))
         );

         using (var sn = context.CreateSession())
         {
            int count;
            var list = sn.Query<Person>()
               .Where(p => p.Address.Line2 == null || p.Address.Line2.AsComparable() > "")
               .Take(10)
               .Skip(5)
               .OrderBy(p => p.Address.City)
               .ToList(out count);
         }
      }
   }
}