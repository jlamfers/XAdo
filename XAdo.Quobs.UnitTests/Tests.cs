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
               .From<Person_Person>()
               .Select(p => new Person {NameFirst = p.FirstName, NameLast = p.LastName})
               .Where(p => p.NameFirst.Contains("e"));
            var sql = mq.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            //var persons = s.From<Person_Person>().AsQueryable();
            //var qq = from p in persons
            //   where p.LastName.StartsWith("A")
            //   orderby p.FirstName, p.LastName, p.EmailPromotion
            //   select new {p.FirstName, p.LastName, p.EmailPromotion};

            //var qlist = qq.ToList();



            mq.ToList();

            var list = s
               .From<Person_Person>()
               .Select(p => new {Name = p.FirstName + " " + p.LastName})
               .ToList();

            var q = s
               .From<Person_Person>()
               .GroupBy(p => p.LastName)
               .Having(p => p.LastName.Count().Between(100, 150))
               .Select(p => new {p.LastName, Count = p.LastName.Count()})
               .OrderByDescending(p => p.Count)
               .AddOrderBy(p => p.LastName);

            sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list2 = q.ToList();
         }
         using (var s = Db.Northwind.CreateSession())
         {
            mq = s.From(mq);
            mq.ToList();
         }

      }

      [Test]
      public void MoneyTest2()
      {

         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<FamilyPerson>()
               .Select(p => new
               {

                  p.Name,
                  Father = p.FatherId != null ? new {p.Father(JoinType.Left).Name} : null,
                  Mother = p.MotherId != null ? new {p.Mother(JoinType.Left).Name} : null,
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
         using (var db = Db.Northwind.CreateSession())
         {
            var trq = db.StartSqlBatch();

            var u = db
               .Update<Person_Person>()
               .From(() => new Person_Person { BusinessEntityID = 968577484, FirstName = "Tim", LastName = "Yep" });


            var sql = u.CastTo<ISqlBuilder>().GetSql();

            var result = u.Apply();

            Debug.WriteLine(sql);

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               u = db
                  .Update<Person_Person>()
                  .From(() => new Person_Person { BusinessEntityID = 989898989, FirstName = "Tim" })
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
         using (var db = Db.Northwind.CreateSession())
         {

            var trq = db.StartSqlBatch();

            db.Delete<FamilyPerson>()
               .Where(p => true)
               .Apply();

            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Insert<FamilyPerson>()
                  .WithArgumentsAsLiterals()
                  .From(() => new FamilyPerson {Id = i, Name = i1.ToString(), FatherId = i1, MotherId = i1})
                  .Apply();
            }

            db.Delete<FamilyPerson>()
               .Where(p => true)
               .Apply();

            db.FlushSqlBatch();
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               var n = i.ToString();
               var i1 = i;
               db
                  .Insert<FamilyPerson>()
                  .WithArgumentsAsLiterals()
                  .From(() => new FamilyPerson {Id = i1, Name = i1.ToString(), FatherId = i1, MotherId = i1})
                  .Apply();
            }
            //Debug.WriteLine(sw.ElapsedMilliseconds);
            db.FlushSqlBatch();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [Test]
      public void MultiInsertTest2()
      {
         using (var db = Db.Northwind.CreateSession())
         {
            var tr = db.BeginTransaction(true);
            var trq = db.StartSqlBatch();

            db.Delete<FamilyPerson>()
               .Where(p => true)
               .Apply();

            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Insert<FamilyPerson>()
                  .WithArgumentsAsLiterals()
                  .From(() => new FamilyPerson {Id = i, Name = i1.ToString(), FatherId = i1, MotherId = i1})
                  .Apply();
            }

            db.Delete<FamilyPerson>()
               .Where(p => true)
               .Apply();

            db.FlushSqlBatch();
            tr.Commit();
            tr = db.BeginTransaction();
            trq = db.StartSqlBatch();


            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               var n = i.ToString();
               var i1 = i;
               db
                  .Insert<FamilyPerson>()
                  .WithArgumentsAsLiterals()
                  .From(() => new FamilyPerson { Id = i1, Name = i1.ToString(), FatherId = i1, MotherId = i1 })
                  .Apply();
            }
            tr.Commit();
            db.FlushSqlBatch();
            //Debug.WriteLine(sw.ElapsedMilliseconds);
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [Test]
      public void MultiInsertTest3()
      {
         using (var db = Db.Northwind.CreateSession())
         {
            var tr = db.BeginTransaction(true);
            //var trq = db.BeginSqlQueue();

            db.Delete<FamilyPerson>()
               .Where(p => true)
               .Apply();

            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Insert(new FamilyPerson
                  {
                     Id = i,
                     Name = i1.ToString(),
                     FatherId = i1,
                     MotherId = i1
                  });
            }

            db.Delete<FamilyPerson>()
               .Where(p => true)
               .Apply();

            //db.FlushSql();
            //tr.Commit();

            


            var sw = new Stopwatch();
            sw.Start();
            //tr = db.BeginTransaction();
            for (var i = 0; i < 1000; i++)
            {
               var n = i.ToString();
               var i1 = i;
               db
                  .Insert(new FamilyPerson
                  {
                     Id = i,
                     Name = i1.ToString(),
                     FatherId = i1,
                     MotherId = i1
                  });
            }
            tr.Commit();
            //db.FlushSql();
            //Debug.WriteLine(sw.ElapsedMilliseconds);
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }
      [Test]
      public void TestEmptyTransaction()
      {
         using (var db = Db.Northwind.CreateSession())
         {
            var tr = db.BeginTransaction(true);
         }
      }
   }
}
