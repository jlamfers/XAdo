using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XAdo.EF;

namespace XAdo.Quobs.UnitTests
{
   [TestFixture]
   public class EfTests
   {
      [Test]
      public void MonkeyTest()
      {
         using (var ctx = new AW())
         {
            var persons = ctx.People;
            var be = ctx.BusinessEntities;
            var q = from p in persons
                    where p.FirstName.StartsWith("e")
                    orderby p.FirstName, p.LastName
                    select new
                    {
                       p.FirstName,
                       p.LastName,
                       BusinessEntity = new
                       {
                          p.BusinessEntity.BusinessEntityID,
                          p.BusinessEntity.ModifiedDate,
                          p.BusinessEntity.rowguid
                       }
                    };
            var list = q.ToList();

            var sw = new Stopwatch();

            sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 10; i++)
            {
               q = from p in persons
                       where p.FirstName.StartsWith("e")
                       orderby p.FirstName, p.LastName
                       select new
                       {
                          p.FirstName,
                          p.LastName,
                          BusinessEntity = new
                          {
                             p.BusinessEntity.BusinessEntityID,
                             p.BusinessEntity.ModifiedDate,
                             p.BusinessEntity.rowguid
                          }
                       };
               list = q.ToList();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);


         }

      }

      [Test]
      public void MonkeyTest2()
      {
         using (var ctx = new AW())
         {
            ctx.Configuration.AutoDetectChangesEnabled = false;
            ctx.Configuration.ValidateOnSaveEnabled = false;
            int id = ctx.FamilyPersons.Max(p => p.Id) + 1;
            var sw = new Stopwatch();
            ctx.FamilyPersons.Add(new FamilyPerson
            {
               Id = id + 1,
               Name = (id + 1).ToString(),
               FatherId = id + 1,
               MotherId = id + 1
            });
            ctx.SaveChanges();
            id++;
            sw.Start();
            for (var i = 1; i <= 1000; i++)
            {
               var p = new FamilyPerson
               {
                  Id = id + i,
                  Name = (id + i).ToString(),
                  FatherId = id + i,
                  MotherId = id + i
               };
               ctx.FamilyPersons.Add(p);
               ctx.SaveChanges();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }
   }
}
