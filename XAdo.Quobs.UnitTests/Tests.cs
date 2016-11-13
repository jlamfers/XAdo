using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbSchema.AdventureWorks;
using NUnit.Framework;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.UnitTests
{
   [TestFixture]
    public class Tests
    {
      [Test]
      public void MoneyTest()
      {

         using (var s = Db.Northwind.CreateSession())
         {
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

            var sql = q.CastTo<ISqlBuilder>().GetSql();

            Debug.WriteLine(sql);

            var list2 = q.ToList();
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
    }
}
