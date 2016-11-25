using System;
using System.Diagnostics;
using System.Linq;
using DbSchema.AdventureWorks;
using NUnit.Framework;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.UnitTests
{

   public class Person
   {
      public string NameFirst { get; set; }
      public string NameLast { get; set; }
   }
   [TestFixture]
    public class Tests
    {
      [Test]
      public void MoneyTest()
      {
         var mq = default(MappedQuob<Person>);
         using (var s = Db.Northwind.CreateSession())
         {
            mq = s
               .From<DbPerson>()
               .Select(p => new Person{NameFirst=p.FirstName, NameLast=p.LastName})
               .Where(p => p.NameFirst.Contains("e"));
            var sql = mq.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var persons = s.From<DbPerson>().AsQueryable();
            var qq = from p in persons
               where p.LastName.StartsWith("A")
               orderby p.FirstName, p.LastName, p.EmailPromotion
               select new {p.FirstName, p.LastName, p.EmailPromotion};

            var qlist = qq.ToList();
               


            mq.ToList();

            var list = s
               .From<DbPerson>()
               .Select(p => new {Name = p.FirstName + " "+p.LastName})
               .ToList();

            var q = s
               .From<DbPerson>()
               .GroupBy(p => p.LastName)
               .Having(p => p.LastName.Count().Between(100,150))
               .Select(p => new {p.LastName, Count = p.LastName.Count()})
               .OrderByDescending(p => p.Count)
               .AddOrderBy(p => p.LastName);

            sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list2 = q.ToList();
         }
         using (var s = Db.Northwind.CreateSession())
         {
            mq = s.Connect(mq);
            mq.ToList();
         }
         
      }

      [Test]
      public void MoneyTest2()
      {

         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<DbFamilyPerson>()
               .Select(p => new
               {
                 
                  p.Name,
                  Father = p.FatherId != null ? new {p.Father(JoinType.Left).Name} : null,
                  Mother = p.MotherId != null ? new {p.Mother(JoinType.Left).Name } : null,
               });

            var sql = q.CastTo<ISqlBuilder>().GetSql();

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               //q = s
               //.From<DbFamilyPerson>()
               //.Select(p => new
               //{

               //   p.Name,
               //   Father = p.FatherId != null ? new { p.Father(JoinType.Left).Name } : null,
               //   Mother = p.MotherId != null ? new { p.Mother(JoinType.Left).Name } : null,
               //});
               //var q3 = q.Clone();
               var q2 = q.Clone().Where(p => p.Father.Name.Contains("K"));
               sql = q2.CastTo<ISqlBuilder>().GetSql();
               
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);

            Debug.WriteLine(sql);
            sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();

         }

      }


      [Test]
      public void MonkeyTest4()
      {
         using (var db = Db.Northwind.CreateSession().BeginUnitOfWork())
         {
            var u = db
               .Update<DbPerson>()
               .Set(() => new DbPerson {BusinessEntityID = 968577484, FirstName = "Tim", LastName = "Yep"});

            
            var sql = u.CastTo<ISqlBuilder>().GetSql();

            var result = u.Apply();

            Debug.WriteLine(sql);

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               u = db
                  .Update<DbPerson>()
                  .Set(() => new DbPerson { BusinessEntityID = 989898989, FirstName = "Tim" })
                  .Where(p => p.FirstName.Contains("Timmetje"));

               sql = u.CastTo<ISqlBuilder>().GetSql();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }

      }

      [Test]
      public void MultiInsertTest()
      {
         using (var db = Db.Northwind.CreateSession().BeginUnitOfWork())
         {
            db.Execute("delete FamilyPerson");
            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Create<DbFamilyPerson>(true)
                  .Add(() => new DbFamilyPerson {Id=i, Name = i1.ToString(), FatherId = i1, MotherId = i1})
                  .Apply();
            }
            db.UnitOfWork.Flush(db);
            var sw = new Stopwatch();
            db.Execute("delete FamilyPerson");
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Create<DbFamilyPerson>(true)
                  .Add(() => new DbFamilyPerson { Id = i, Name = i1.ToString(), FatherId = i1, MotherId = i1 })
                  .Apply();
            }
            db.UnitOfWork.Flush(db);
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }
    }
}
