using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
               orderby p.FirstName, p.LastName  
                      select new{p.FirstName,p.LastName, BusinessEntity = new{p.BusinessEntity().BusinessEntityID}};

            var q2 = from x in q where x.BusinessEntity.BusinessEntityID != null select x;
            ;
               //select new {p.FirstName, p.LastName};

            var list = q2.ToList();



         }
         
      }
   }
}
