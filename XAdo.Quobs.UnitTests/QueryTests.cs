using System.Diagnostics;
using System.Linq;
using DbSchema.AdventureWorks;
using NUnit.Framework;
using XAdo.Core.Interface;

namespace XAdo.Quobs.UnitTests
{
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
      public void ToListWorks()
      {
         _db.From<DbStateProvince>().ToList();
      }
      [Test]
      public void ToArrayWorks()
      {
         _db.From<DbStateProvince>().ToArray();
      }
      [Test]
      public void ToDictionaryWorks()
      {
         _db.From<DbStateProvince>().ToDictionary(r => r.StateProvinceID, r => r);
      }

      [Test]
      public void AnyWorks()
      {
         Assert.IsTrue(_db.From<DbStateProvince>().Any());
         Assert.IsTrue(_db.From<DbStateProvince>()
            .Where(p => p.Name != null)
            .OrderBy(p => p.Name)
            .Skip(1)
            .Take(5)
            .Any());
         Assert.IsFalse(_db.From<DbStateProvince>()
            .Where(p => p.Name != null)
            .OrderBy(p => p.Name)
            .Skip(int.MaxValue-5)
            .Take(5)
            .Any());
      }
      [Test]
      public void AnyPerformance()
      {
         var sw = default(Stopwatch);
         var q = _db.From<DbStateProvince>();
         
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
            var b = q.ToEnumerable().Any();
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
         var q = _db.From<DbStateProvince>().Any(p => p.CountryRegionCode != null);
      }
      [Test]
      public void ToListWithCountWorks()
      {
         int count;
         var list = _db.From<DbStateProvince>().ToList(out count);
         Assert.AreEqual(count,list.Count);
         Assert.AreEqual(count, _db.From<DbStateProvince>().Count());
      }
      [Test]
      public void PagingWorksWithSkipOnly()
      {
         int count;
         var list = _db
            .From<DbStateProvince>()
            .Skip(10)
            .OrderBy(p => p.CountryRegionCode)
            .ToList(out count);
         Assert.AreEqual(count, list.Count + 10);

         var count2 = _db
            .From<DbStateProvince>()
            .Skip(10)
            .OrderBy(p => p.CountryRegionCode)
            .Count();
         Assert.AreEqual(count2, list.Count);
      }
      [Test]
      public void PagingWorksWithTakeOnly()
      {
         var list = _db
            .From<DbStateProvince>()
            .Take(10)
            .OrderBy(p => p.CountryRegionCode)
            .ToList();
         Assert.AreEqual(10, list.Count);
      }
      [Test]
      public void PagingWorksWithBothTakeAndSkip()
      {
         var list = _db
            .From<DbStateProvince>()
            .Skip(1)
            .Take(10)
            .OrderBy(p => p.CountryRegionCode)
            .ToList();
         Assert.AreEqual(10, list.Count);
         var list2 = _db
            .From<DbStateProvince>()
            .OrderBy(p => p.CountryRegionCode)
            .ToArray()
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
