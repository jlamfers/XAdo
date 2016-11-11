using System;
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
      public void JoinTest()
      {
         foreach(var m in typeof(JoinExtension).GetMethods(BindingFlags.Public | BindingFlags.Static))
         {
            var join1 = m.GetJoinDescriptors(JoinType.Inner).First();
            //var join2 = m.GetJoinDescriptors2().First();
            //Assert.AreEqual(join1.Expression,join2.Expression);
         }
      }
    }
}
