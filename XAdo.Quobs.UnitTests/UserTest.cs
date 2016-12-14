using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            db.Execute("delete dbo.[usertype]");
            db.Execute("delete dbo.[user]");
            db.Execute("delete dbo.[role]");
            db.Execute("delete dbo.[type]");
            var users = new List<User>();
            var roles = new List<Role>();
            var types = new List<Type>();
            foreach (var user in new[] { "Frank", "Klaas", "Piet", "Wim", "Admin" })
            {
               var usr = new User {UserName = user, Password = "test"};
               db.Insert(usr);
               users.Add(usr);
            }
            db.Insert<User>()
               .From(() => new User {UserName = "Jan", Password = "test"})
               .Apply();
            foreach (var role in new[] { "Admin", "User", "Manager", "Buyer", "Seller" })
            {
               var r = new Role {Name = role};
               db.Insert(r);
               roles.Add(r);
            }
            foreach (var type in new[] { "Normal", "Special" })
            {
               var t = new Type { Name = type };
               db.Insert(t);
               types.Add(t);
            }
            foreach (var u in users)
            {
               foreach (var r in roles)
               {
                  db.Insert<UserRole>()
                     .From(() => new UserRole {RoleId = r.Id, UserId = u.Id})
                     .Apply();
               }
               foreach (var t in types)
               {
                  db.Insert<UserType>()
                     .From(() => new UserType { TypeId = t.Id, UserId = u.Id })
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
            var q = db.From<User>()
               .Map(u => new
               {
                  u.UserName,
                  u.Password,
                  Role = u.UserRole_N(JoinType.Left).DefaultIfEmpty(ur => new
                  {
                     ur.RoleId,
                     ur.Role(JoinType.Left).Name
                  }),
                  Type = u.UserType_N(JoinType.Left).DefaultIfEmpty(ur => new
                  {
                     ur.TypeId,
                     ur.Type(JoinType.Left).Name
                  })
               });

            var sql = q.CastTo<ISqlBuilder>().GetSql();
            Debug.WriteLine(sql);

            var list = q.FetchToList();
         }
        
      }

      [Test]
      public void MonkeyTest2()
      {
         using (var db = Db.Users.CreateSession())
         {
            var users = db.From<User>()
               .Map(u => new
               {
                  Id=u.Id.Value,
                  u.UserName,
                  u.Password,
                  Roles = new List<string>(10),
                  Types = new List<string>(10)
               })
               .OrderBy(u => u.UserName)
               .Skip(1)
               .Take(5)
               .FetchToList();

            var first = users.First().UserName;
            var last = users.Last().UserName;

            var roles = db.From<User>()
               .Where(u => u.UserName.Between(first, last))
               .Map(u => new
               {
                  UserId = u.Id,
                  u.UserRole_N(JoinType.Left).Role(JoinType.Left).Name,
               })
               .FetchToGroupedDictionary(u => u.UserId, u => u.Name);

            var types = db.From<User>()
               .Where(u => u.UserName.Between(first,last))
               .Map(u => new
               {
                  UserId = u.Id,
                  u.UserType_N(JoinType.Left).Type(JoinType.Left).Name,
               })
               .FetchToGroupedDictionary(u => u.UserId, u => u.Name);


            foreach (var user in users)
            {
               user.Roles.AddRange(roles[user.Id]);
               user.Types.AddRange(types[user.Id]);
            }
         }

      }

      [Test]
      public void PerformanceMonkeyTest2()
      {
         MonkeyTest2();
         var sw = new Stopwatch();
         sw.Start();
         MonkeyTest2();
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }
   }
}
