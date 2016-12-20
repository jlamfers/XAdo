﻿using System;
using System.Diagnostics;
using System.Linq;
using DbSchema;
using NUnit.Framework;
using XAdo.Core.Interface;
using XAdo.SqlObjects;
using XAdo.SqlObjects.DbSchema.Attributes;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects;
using XAdo.SqlObjects.SqlObjects.Interface;

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
         MappedSqlObject<Person> mq = null;
         using (var s = Db.Northwind.CreateSession())
         {
            mq = s
               .From<AW.Person.Person_>()
               .Map(p => new Person {NameFirst = p.FirstName, NameLast = p.LastName})
               .Where(p => p.NameFirst.Contains("e"));
            var sql = mq.CastTo<ISqlObject>().GetSql();
            Debug.WriteLine(sql);

            //var persons = s.From<AW.Person.Person_>().AsQueryable();
            //var qq = from p in persons
            //   where p.LastName.StartsWith("A")
            //   orderby p.FirstName, p.LastName, p.EmailPromotion
            //   select new {p.FirstName, p.LastName, p.EmailPromotion};

            //var qlist = qq.ToList();



            mq.FetchToList();

            var list = s
               .From<AW.Person.Person_>()
               .Map(p => new {Name = p.FirstName + " " + p.LastName})
               .FetchToList();

            var q = s
               .From<AW.Person.Person_>()
               .GroupBy(p => p.LastName)
               .Having(p => p.LastName.Count().Between(100, 150))
               .Map(p => new { p.LastName, Count = p.LastName.Count() })
               .OrderByDescending(p => p.Count)
               .AddOrderBy(p => p.LastName);

            sql = q.CastTo<ISqlObject>().GetSql();
            Debug.WriteLine(sql);

            var list2 = q.FetchToList();
         }
         using (var s = Db.Northwind.CreateSession())
         {
            var mq2 = s.From(mq);
            mq2.FetchToList();
         }

      }

      [Test]
      public void MoneyTest2()
      {

         using (var s = Db.Northwind.CreateSession())
         {
            var q = s
               .From<AW.FamilyPerson>()
               .Map(p => new
               {

                  p.Name,
                  Father = p.FatherId != null ? new {p.Father(JoinType.Left).Name} : null,
                  Mother = p.MotherId != null ? new {p.Mother(JoinType.Left).Name} : null,
               });

            var sql = q.CastTo<ISqlObject>().GetSql();

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
               sql = q2.CastTo<ISqlObject>().GetSql();

            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);

            Debug.WriteLine(sql);
            sql = q.CastTo<ISqlObject>().GetSql();
            Debug.WriteLine(sql);

            var list = q.FetchToList();

         }

      }


      [Test]
      public void MonkeyTest4()
      {
         using (var db = Db.Northwind.CreateSession())
         {
            var trq = db.StartSqlBatch();

            var u = db
               .Update<AW.Person.Person_>()
               .From(() => new AW.Person.Person_ { BusinessEntityID = 968577484, FirstName = "Tim", LastName = "Yep" });


            var sql = u.CastTo<ISqlObject>().GetSql();

            object result = null;
            
            u.Apply(callback:r => result = r);

            Debug.WriteLine(sql);

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               u = db
                  .Update<AW.Person.Person_>()
                  .From(() => new AW.Person.Person_ { BusinessEntityID = 989898989, FirstName = "Tim" })
                  .Where(p => p.FirstName.Contains("Timmetje"));

               sql = u.CastTo<ISqlObject>().GetSql();
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

            db.Delete<AW.FamilyPerson>()
               .Where(p => true)
               .Apply();

            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Insert<AW.FamilyPerson>()
                  .From(() => new AW.FamilyPerson { Id = i, Name = i1.ToString(), FatherId = i1, MotherId = i1 })
                  .Apply(literals:true);
            }

            db.Delete<AW.FamilyPerson>()
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
                  .Insert<AW.FamilyPerson>()
                  .From(() => new AW.FamilyPerson { Id = i1, Name = i1.ToString(), FatherId = i1, MotherId = i1 })
                  .Apply(literals: true);
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

            db.Delete<AW.FamilyPerson>()
               .Where(p => true)
               .Apply();

            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Insert<AW.FamilyPerson>()
                  .From(() => new AW.FamilyPerson { Id = i, Name = i1.ToString(), FatherId = i1, MotherId = i1 })
                  .Apply(literals:true);
            }

            db.Delete<AW.FamilyPerson>()
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
                  .Insert<AW.FamilyPerson>()
                  .From(() => new AW.FamilyPerson { Id = i1, Name = i1.ToString(), FatherId = i1, MotherId = i1 })
                  .Apply(literals:true);
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

            db.Delete<AW.FamilyPerson>()
               .Where(p => true)
               .Apply();

            for (var i = 0; i < 1000; i++)
            {
               var i1 = i;
               db
                  .Insert(new AW.FamilyPerson
                  {
                     Id = i,
                     Name = i1.ToString(),
                     FatherId = i1,
                     MotherId = i1
                  });
            }

            db.Delete<AW.FamilyPerson>()
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
                  .Insert(new AW.FamilyPerson
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
