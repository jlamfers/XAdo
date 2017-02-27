using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using XAdo.Sql;
using XAdo.Sql.Core;
using XPression.Core;

namespace XAdo.UnitTest
{
   public static class Constants
   {
      public const string SqlSelect = @"
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
      public async void MonkeyTest()
      {
         var context = new SqlAdoContext(cfg => cfg.SetConnectionStringName("AW"));

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query<Person>().Where(p => p.FirstName != null).OrderBy(p => p.Id).Skip(10).Take(10).Fetch();
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
      public async void MonkeyTest2()
      {
         var context = new SqlAdoContext(cfg => cfg.SetConnectionStringName("AW"));

         using (var sn = context.CreateSession())
         {
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
         var context = new SqlAdoContext(cfg => cfg.SetConnectionStringName("AW"));

         using (var sn = context.CreateSession())
         {
            var qb = sn.GetQueryBuilder(Constants.SqlSelect);
            qb.GetBinder(sn);
            var sql =
               qb.Format(
                  new
                  {
                     where = qb.BuildSqlPredicate("firstname ne null", null).Sql,
                     order = qb.GetSqlOrderBy("id, -firstname, lastname", null),
                     skip = 10,
                     take = 10
                  });
            sn.Query(sql,qb.GetBinder(sn));

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
   }
}
