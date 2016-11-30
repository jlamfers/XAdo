using System.Collections.Generic;
using System.Diagnostics;
using DbSchema.Users;
using NUnit.Framework;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.UnitTests
{
   [TestFixture]
   public class UserTest
   {

      [SetUp]
      public void Setup()
      {
         using (var db = Db.Users.CreateSession())
         {
            var tr = db.BeginTransaction(true);
            db.Execute("delete dbo.[userrole]");
            db.Execute("delete dbo.[user]");
            db.Execute("delete dbo.[role]");
            var users = new List<DbUser>();
            var roles = new List<DbRole>();
            foreach (var user in new[] {"Jaap", "Klaas", "Piet", "Wim", "Admin"})
            {
               var usr = new DbUser {UserName = user, Password = "test"};
               db.Insert(usr);
               users.Add(usr);
            }
            db.Insert<DbUser>()
               .From(() => new DbUser {UserName = "Jan", Password = "test"})
               .Apply();
            foreach (var role in new[] { "Admin", "User", "Manager", "Buyer", "Seller" })
            {
               var r = new DbRole {Name = role};
               db.Insert(r);
               roles.Add(r);
            }
            foreach (var u in users)
            {
               foreach (var r in roles)
               {
                  db.Insert<DbUserRole>()
                     .From(() => new DbUserRole {RoleId = r.Id, UserId = u.Id})
                     .Apply();
               }
            }
         }
      }

      [Test]
      public void MonkeyTest()
      {
         using (var db = Db.Users.CreateSession())
         {
            var q = db.From<DbUser>()
               .Select(u => new
               {
                  u.UserName,
                  u.Password,
                  Role = u.UserRole_N(JoinType.Left).DefaultIfEmpty(ur => new
                  {
                     ur.RoleId,
                     ur.Role(JoinType.Left).Name
                  })
               });

            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.ToList();
         }
         
      }
   }
}
