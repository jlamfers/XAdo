using System.Diagnostics;
using System.Linq;
using DbSchema.AdventureWorks;
using NUnit.Framework;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.UnitTests
{
   [TestFixture]
   public class QueryableTest
   {

      [Test]
      public void MonkeyTest()
      {
         using (var db = Db.Northwind.CreateSession())
         {
            var persons = new QueryableQuob<DbPerson>(db.From<DbPerson>());

            var q = from p in persons
               where p.FirstName.Contains("e")
               //orderby p.FirstName, p.LastName  
                      select new{p.FirstName,p.LastName, BusinessEntity = new
                      {
                         p.BusinessEntity().BusinessEntityID,
                         p.BusinessEntity().ModifiedDate,
                         p.BusinessEntity().rowguid
                      }};


            var q2 = from x in q where x.BusinessEntity.BusinessEntityID != null orderby x.FirstName,x.LastName,x.BusinessEntity.BusinessEntityID select x;

            var q3 = from y in q2 
                     select new {y.FirstName, y.LastName};

            var q4 = from z in q3
               orderby z.LastName, z.FirstName
               select new {z.FirstName, z.LastName};
            
               //select new {p.FirstName, p.LastName};

            var list = q4.ToList();



         }
         
      }

      [Test]
      public void PerfTest()
      {
         using (var db = Db.Northwind.CreateSession())
         {
            var persons = new QueryableQuob<DbPerson>(db.From<DbPerson>());

            var q = from p in persons
                    where p.FirstName.StartsWith("e")
                    orderby p.FirstName, p.LastName  
                    select new
                    {
                       p.FirstName,
                       p.LastName,
                       BusinessEntity = new
                       {
                          p.BusinessEntity().BusinessEntityID,
                          p.BusinessEntity().ModifiedDate,
                          p.BusinessEntity().rowguid
                       }
                    };

            q.ToList();
            var list = q.Where(p => p.BusinessEntity.BusinessEntityID != null).ToList();

            var q2 = db.From<DbPerson>()
               .Where(p => p.FirstName.StartsWith("e"))
               .OrderBy(p => p.FirstName, p => p.LastName)
               .Select(p => new
               {
                  p.FirstName,
                  p.LastName,
                  BusinessEntity = new
                  {
                     p.BusinessEntity().BusinessEntityID,
                     p.BusinessEntity().ModifiedDate,
                     p.BusinessEntity().rowguid
                  }
               });

            q2.ToList();

            var sw = new Stopwatch();

            sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 10; i++)
            {
               q.Where(p => p.BusinessEntity.BusinessEntityID != null).ToList();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);

            sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 10; i++)
            {
               q2.Where(p => p.BusinessEntity.BusinessEntityID != null).ToList();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);

         }

      }

   }
}
