using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using XAdo.Core.Impl;
using XAdo.DbSchema;
using XAdo.Quobs;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Impl;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;
using XAdo.Quobs.Providers;
using XPression.Core;
using Scanner = XAdo.Quobs.Core.Impl.SqlScannerImpl;

namespace XAdo.UnitTest
{
   public static class Constants
   {
      public const string SqlSelect = @"
SELECT        
   p.BusinessEntityID as id -->*
   ,p.PersonType -->! 
   ,p.Title 
   ,p.FirstName -->! 
   ,p.MiddleName 
   ,p.LastName -->! 
   ,p.ModifiedDate as ModifiedAt 
   ,a.AddressID --> Address/Id* 
   ,a.AddressLine1 as Line1 -->!
   ,a.AddressLine2 as Line2 
   ,a.PostalCode -->! 
   ,a.ModifiedDate as ModifiedAt
   ,at.AddressTypeID -->AddressType/Id*
   ,at.Name -->!
   ,a.City -->../City!
FROM Person.BusinessEntity AS be -->@
INNER JOIN Person.Person AS p ON be.BusinessEntityID = p.BusinessEntityID 
INNER JOIN Person.BusinessEntityAddress AS bea ON be.BusinessEntityID = bea.BusinessEntityID 
INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID 
INNER JOIN Person.AddressType AS at ON bea.AddressTypeID = at.AddressTypeID
";
   }
   [SqlSelect(Constants.SqlSelect)]
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
      [Test]
      public void ParseTest()
      {
         var p = new SqlSelectParserImpl(new Scanner());
         var result = p.Parse(Constants.SqlSelect).EnsureLinked();
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            p.Parse(Constants.SqlSelect).EnsureLinked();
         }
         sw.Stop();
         result.OfType<SelectPartial>().Single().Columns.First().Table.SetAlias("oops");
         Debug.WriteLine(sw.ElapsedMilliseconds);
         var q = new SqlResourceImpl(result, new SqlServerDialect(), new UrlFilterParserImpl(),
            new SqlPredicateGeneratorImpl(new SqlDialectImpl()), new SqlTemplateFormatterImpl(), new SqlBuilderImpl());

      }

      [Test]
      public void MonkeyTest()
      {
         var context = new QuobsContext(cfg => cfg
            .SetConnectionStringName("AW")
         );

         using (var sn = context.CreateSession())
         {
            var schema = sn.GetDbSchema();

            var sorted = schema.Tables.SortDeleteOrder();
           

            var copy = sorted.ToList();
            foreach (var t in sorted)
            {
               var childs = t.ChildTables;
               var result = childs.Any(c => c != t && copy.Contains(c));
               if (result)
               {
                  throw new Exception("expected false");
               }
               copy.Remove(t);
            }

            var persons = sn.Query<Person>().Where(p => p.FirstName != null).OrderBy(p => p.Id).Skip(10).Take(10).Fetch();
            var count = sn.Query<Person>().Where(p => p.FirstName != null).OrderBy(p => p.Id).Skip(10).Take(10).TotalCount();

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               var persons2 = sn.Query<Person>().Where(p => p.FirstName != null).OrderBy(p => p.Id).Skip(10).Take(10);
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [Test]
      public void MonkeyTest2()
      {
         var context = new QuobsContext(cfg => cfg.SetConnectionStringName("AW").EnableDbSchema());

         using (var sn = context.CreateSession())
         {
            var pc = sn.Query<Person>().CastTo<IQuob>().Where("firstname ne null").OrderBy("id").Skip(10).Take(10);

            var persons = sn.Query<Person>().CastTo<IQuob>().Where("firstname ne null").OrderBy("id").Skip(10).Take(10).Fetch();
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               var persons2 = sn.Query<Person>().CastTo<IQuob>().Where("firstname ne null").OrderBy("id").Skip(10).Take(10);
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [Test]
      public async void MonkeyTest3()
      {
         var context = new QuobsContext(cfg => cfg.SetConnectionStringName("AW"));

         var persistBuilder = new SqlPersistBuilder();

         using (var sn = context.CreateSession())
         {
            sn.BeginTransaction();
            var qb = sn.GetSqlResource(Constants.SqlSelect);
            var sqlSelect = new SqlBuilderImpl().BuildSelect(qb);
            var upd = persistBuilder.BuildUpdate(qb);
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               upd = persistBuilder.BuildUpdate(qb);
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
            qb.GetBinder(sn);
            var sql =
               qb.BuildSqlSelect(
                  new
                  {
                     where = qb.BuildSqlPredicate("firstname ne null", null).Sql,
                     order = qb.BuildSqlOrderBy("id, -firstname, lastname", null),
                     skip = 10,
                     take = 10
                  });
            var first = sn.Query(sql,qb.GetBinder(sn)).First();

            var result = sn.Execute(upd, first);
            sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               result = sn.Execute(upd, first);
            }
            sw.Stop();
            Debug.WriteLine("Updated: "+sw.ElapsedMilliseconds);

            sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               var persons2 = sn.Query<Person>().CastTo<IQuob>().Where("firstname ne null").OrderBy("id").Skip(10).Take(10);
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [Test]
      public void RegExTest()
      {
         var m = new ColumnMeta();
         m.InitializeByTag("* {'onupdate':'input','type':'decimal','maxlength':10}",false);
      }

      [Test]
      public void SortTest()
      {
         var context = new QuobsContext(cfg => cfg
            .SetConnectionStringName("AW")
            .EnableDbSchema()
         );
         DbSchema.DbSchema schema;
         using (var sn = context.CreateSession())
         {
            schema = sn.GetDbSchema();
         }

         var sorted = schema.Tables.SortDeleteOrder();

         var copy = sorted.ToList();
         foreach (var t in sorted)
         {
            var childs = t.ChildTables;
            var result = childs.Any(c => c != t && copy.Contains(c));
            if (result)
            {
               throw new Exception("expected false");
            }
            copy.Remove(t);
         }

         sorted = schema.Tables.SortInsertOrder();

         copy = new List<DbTableItem>();
         foreach (var t in sorted)
         {
            var childs = t.Columns.Where(c => c.References != null && c.References.Table!=t).Select(c => c.References.Table).ToList();
            var result = childs.All(c => copy.Contains(c));
            if (!result)
            {
               throw new Exception("expected false");
            }
            copy.Add(t);
         }

         
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            schema.Tables.SortDeleteOrder();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }
   }
}
