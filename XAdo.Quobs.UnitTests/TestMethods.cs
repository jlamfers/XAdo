using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using DbSchema.AdventureWorks;
using NUnit.Framework;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.UnitTests
{
   [TestFixture]
   public class TestMethods
   {
      [Test]
      public void MonkeyTest()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<DbPerson>()
               .Distinct()
               .OrderBy(p => p.LastName)
               .Take(100)
               .Skip(10)
               .Select(p => new
               {
                  p.FirstName,
                  p.LastName,
                  Id = p.BusinessEntityID + p.BusinessEntityID,
                  FullName = p.FirstName + " " + (p.LastName ?? "")
               })
               .OrderBy(p => p.Id)
               .Where(p => p.Id > 10);


            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();
         }
      }

      [Test]
      public void MonkeyTest2()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var dict = new ConcurrentDictionary<int, object>();
            var q = s
               .From<DbBusinessEntityContact>()
               .Take(100)
               .Skip(10)
               .Select(p => new
               {
                  p.Person().FirstName,
                  p.Person().LastName,
                  BusinessEntity = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID != null ? new
                  {
                     BusinessEntityID = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID,
                     p.Person().BusinessEntity().ModifiedDate
                  } : null,
                  BusinessEntity2 = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID != null ?
                  dict.GetOrAdd(p.Person().BusinessEntity(JoinType.Left).BusinessEntityID.Value, x => new
                  {
                     BusinessEntityID = x,
                     p.Person().BusinessEntity().ModifiedDate
                  }) : null
               })
               .OrderBy(p => p.BusinessEntity.ModifiedDate);


            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();
         }
      }

      [Test]
      public void MonkeyTest22()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var dict = new ConcurrentDictionary<int, object>();
            var q = s
               .From<DbBusinessEntityContact>()
               .Take(100)
               .Skip(10)
               .Select(p => new
               {
                  p.Person().FirstName,
                  p.Person().LastName,
                  BusinessEntity = p
                     .Person()
                     .BusinessEntity(JoinType.Left)
                     .BusinessEntityID
                     .DefaultIfEmpty(() => new
                     {
                        BusinessEntityID = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID.Value,
                        p.Person().BusinessEntity().ModifiedDate
                     }),
               })
               .OrderBy(p => p.BusinessEntity.ModifiedDate);


            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();
         }
      }

      public class Person
      {
         public string FirstName { get; set; }
         public string LastName { get; set; }
         public BusinessEntity BusinessEntity { get; set; }
         public BusinessEntity BusinessEntity2 { get; set; }

      }
      public class BusinessEntity
      {
         public int BusinessEntityID { get; set; }
         public DateTime ModifiedDate { get; set; }
      }
      [Test]
      public void MonkeyTest3()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var dict = new ConcurrentDictionary<int, object>();
            var q = s
               .From<DbBusinessEntityContact>()
               .Take(100)
               .Skip(10)
               .Select(p => new Person()
               {
                  FirstName = p.Person().FirstName,
                  LastName = p.Person().LastName,
                  BusinessEntity = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID != null ? new BusinessEntity
                  {
                     BusinessEntityID = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID.Value,
                     ModifiedDate = p.Person().BusinessEntity().ModifiedDate.Value
                  } : null,
                  BusinessEntity2 = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID != null ?
                  (BusinessEntity)dict.GetOrAdd(p.Person().BusinessEntity(JoinType.Left).BusinessEntityID.Value, x => new BusinessEntity
                  {
                     BusinessEntityID = x,
                     ModifiedDate = p.Person().BusinessEntity().ModifiedDate.Value
                  }) : null
               })
               .OrderBy(p => p.BusinessEntity.ModifiedDate);


            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();
         }
      }

      /*
var query = from product in products
                group product by product.Style into g
                select new
                {
                    Style = g.Key,
                    AverageListPrice =
                        g.Average(product => product.ListPrice)
                };       */
      [Test]
      public void MonkeyTest4()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<DbProduct>()
               .GroupBy(p => p.ProductModel().Name)
               .Select(p => new { p.ProductModel().Name, AvgPrice = Math.Round(p.ListPrice.Avg().Value, 2) })
               .OrderBy(p => p.AvgPrice);

            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();
         }
      }

      [Test]
      public void MonkeyTest5()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<DbProduct>()
               .GroupBy(p => p.ProductModel().Name)
               .Select(p => new { p.ProductModel().Name, AvgPrice = Math.Round(p.ListPrice.Avg().Value, 2) })
               .OrderBy(p => p.AvgPrice);

            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var @any = q.Any();


         }
      }

      [Test]
      public void MonkeyTest6()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var q = s.From<DbProduct>().ToEnumerable();
            bool @any = q.Any();
            var list = q.ToList();
         }
      }

      [Test]
      public void MonkeyTest7()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<DbProduct>()
               .Select(p => new { FullName = "_" + p.ProductModel().Name })
               .Union(s.From<DbPerson>().Select(p => new { FullName = p.FirstName + " " + p.LastName }))
               .Distinct()
               .OrderBy(p => p.FullName);

            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();


         }
      }

      [Test]
      public void MonkeyTest8()
      {
         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<DbPerson>()
               .Select(p => new
               {
                  p.FirstName,
                  p.LastName,
                  p.BusinessEntityContact_N().BusinessEntity().BusinessEntityAddress_N().Address().AddressLine1,
                  p.BusinessEntityContact_N().BusinessEntity().BusinessEntityAddress_N().Address().AddressLine2,
                  p.BusinessEntityContact_N().BusinessEntity().BusinessEntityAddress_N().Address().City
               });

            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();


         }
      }
   }
}
