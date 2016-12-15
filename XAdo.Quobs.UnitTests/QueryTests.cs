using System;
using System.Diagnostics;
using System.Linq;
using DbSchema.AdventureWorks;
using NUnit.Framework;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.DbSchema;
using XAdo.Quobs.DbSchema.Attributes;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.UnitTests
{
   public static class CustomJoins
   {
      static CustomJoins()
      {
         //DbSchemaDescriptor.DefineJoin<DbProduct,DbSalesOrderDetail>("myjoin",(l,r) => l.ProductID == r.ProductID && l.Color=="red");
      }
      [JoinMethod("myjoin")]
      public static Sales_SalesOrderDetail RedProducts(this Production_Product product)
      {
         return product.RedProducts(JoinType.Inner);
      }

      [JoinMethod("myjoin")]
      public static Sales_SalesOrderDetail RedProducts(this Production_Product product, JoinType joinType)
      {
         return DbSchemaDescriptor.DefineJoin<Production_Product, Sales_SalesOrderDetail>("myjoin", (l, r) => l.ProductID == r.ProductID && l.Color == "red");
      }
   }
   [TestFixture]
   public class QueryTests
   {
      private IAdoSession _db;

      [SetUp]
      public void Setup()
      {
         _db = Db.Northwind.CreateSession();
      }

      [TearDown]
      public void TearDown()
      {
         _db.Dispose();
      }

      [Test]
      public void CustomJoinWorks()
      {
         //DbSchemaDescriptor.DefineJoin<DbProduct, DbSalesOrderDetail>("myjoin", (l, r) => l.ProductID == r.ProductID && l.Color == "red");

         var q = _db
            .From<Production_Product>()
            .Map(p => new
            {
               p.Class,
               p.Color,
               p.RedProducts(JoinType.Left).UnitPrice,
               p.RedProducts(JoinType.Left).ProductSpecialOffer().ModifiedDate
            })
            .Where(x => x.UnitPrice == null || x.UnitPrice != null || x.ModifiedDate < DateTime.Now)
            .OrderBy(p => p.Class)
            .Skip(10)
            .Take(100);


         var sql = q.CastTo<ISqlObject>().GetSql();
         Debug.WriteLine(sql);

         var list = q.FetchToList();


      }

      [Test]
      public void ToListWorks()
      {
         _db.From<Person_StateProvince>().FetchToList();
      }
      [Test]
      public void ToArrayWorks()
      {
         _db.From<Person_StateProvince>().FetchToArray();
      }
      [Test]
      public void ToDictionaryWorks()
      {
         _db.From<Person_StateProvince>().FetchToDictionary(r => r.StateProvinceID, r => r);
      }

      [Test]
      public void AnyWorks()
      {
         Assert.IsTrue(_db.From<Person_StateProvince>().Any());
         Assert.IsTrue(_db.From<Person_StateProvince>()
            .Where(p => p.Name != null)
            .OrderBy(p => p.Name)
            .Distinct()
            .Skip(1)
            .Take(5)
            .Any());
         Assert.IsTrue(_db.From<Person_StateProvince>()
            .Where(p => p.Name != null)
            .OrderBy(p => p.Name)
            .Distinct()
            .Any());
         Assert.IsFalse(_db.From<Person_StateProvince>()
            .Where(p => p.Name =="kahdjhg")
            .OrderBy(p => p.Name)
            .Skip(10000000)
            .Take(5)
            .Any());
         Assert.IsFalse(_db.From<Person_StateProvince>()
            .OrderBy(p => p.Name)
            .Take(0)
            .Any());
      }
      [Test]
      public void AnyPerformance()
      {
         var sw = default(Stopwatch);
         var q = _db.From<Person_StateProvince>();
         
         q.Any();
         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 100; i++)
         {
            q.Any();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 100; i++)
         {
            var b = q.FetchToEnumerable().Any();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 100; i++)
         {
            var b = q.Count() > 0;
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }

      [Test]
      public void AnyWorksParameterized()
      {
         var q = _db.From<Person_StateProvince>().Where(p => p.CountryRegionCode != null).Any();
      }
      [Test]
      public void ToListWithCountWorks()
      {
         int count;
         var list = _db.From<Person_StateProvince>().FetchToList(out count);
         Assert.AreEqual(count,list.Count);
         Assert.AreEqual(count, _db.From<Person_StateProvince>().Count());
      }
      [Test]
      public void PagingWorksWithSkipOnly()
      {
         int count;
         var list = _db
            .From<Person_StateProvince>()
            .Skip(10)
            .OrderBy(p => p.CountryRegionCode)
            .FetchToList(out count);
         Assert.AreEqual(count, list.Count + 10);

         var count2 = _db
            .From<Person_StateProvince>()
            .Skip(10)
            .OrderBy(p => p.CountryRegionCode)
            .Count();
         Assert.AreEqual(count2, list.Count);
      }
      [Test]
      public void PagingWorksWithTakeOnly()
      {
         var list = _db
            .From<Person_StateProvince>()
            .Take(10)
            .OrderBy(p => p.CountryRegionCode)
            .FetchToList();
         Assert.AreEqual(10, list.Count);
      }
      [Test]
      public void PagingWorksWithBothTakeAndSkip()
      {
         var list = _db
            .From<Person_StateProvince>()
            .Skip(1)
            .Take(10)
            .OrderBy(p => p.CountryRegionCode)
            .FetchToList();
         Assert.AreEqual(10, list.Count);
         var list2 = _db
            .From<Person_StateProvince>()
            .OrderBy(p => p.CountryRegionCode)
            .FetchToArray()
            .Skip(1)
            .Take(10)
            .ToList();
         for (var i = 0; i < 10; i++)
         {
            Assert.AreEqual(list[i].CountryRegionCode,list2[i].CountryRegionCode);
         }

      }
   }
}
