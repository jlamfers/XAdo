using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using NUnit.Framework;
using XAdo.Sql;
using XAdo.Sql.Core;
using XAdo.Sql.Providers;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class SqlTest
   {

      public class Foo
      {
         public int Id { get; protected set; }
         public string Name { get; set; }
         public DateTime Born { get; set; }
      }
      [Test]
      public void MonkeyTest()
      {
         var sql = @"
SELECT
 rowguid as ID,
 FirstName+' '+LastName as Name,
 Born
FROM
 Person
--$WHERE {where}
";
         var sp = new SqlSelectParser();
         var tuples = sp.Parse(sql);
         var map = tuples.Columns.ToDictionary(t => t.Alias, t => t);
         var b = new SqlBuilder(new SqlServerDialect(),null,true);
         string oops="";
         var result = b.Parse(Predicate<Foo>(e => e.Born < DateTime.Now && (e.Name ?? oops) != null && e.Name.CompareTo("a") > 0 || e.Name.Contains("oops")),map,null);
         var resultSql = sql.FormatSqlTemplate(new {where = result.Sql});
         Debug.WriteLine(resultSql);
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            b.Parse(Predicate<Foo>(e => e.Born < DateTime.Now && (e.Name ?? oops) != null && e.Name.AsComparable() > "a" || e.Name.Contains("oops")), map,null);
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }

      private static Expression<Func<T, bool>> Predicate<T>(Expression<Func<T, bool>> e)
      {
         return e;
      }
      private static Expression<Func<IDataRecord,T>> Predicate2<T>(Expression<Func<IDataRecord,T>> e)
      {
         return e;
      }

      [Test]
      public void SerializeTest()
      {
         var d = new SqlServerDialect();
         var result = JsonConvert.SerializeObject(d, Formatting.Indented);
         var result2 = JsonConvert.SerializeObject(new SqlDialect(), Formatting.Indented);

         var des = JsonConvert.DeserializeObject<SqlDialect>(result);
      }

      [Test]
      public void MonkeyTest2()
      {
         var sql = @"
SELECT
 rowguid as Id,
 FirstName+' '+LastName as Name,
 Born
FROM
 Person
--$WHERE {where}
";
         //var exp = Predicate2<Foo>(r => new Foo
         //{
         //   Id = r.GetInt32(0),
         //   Name = r.GetNString(1),
         //   Born = r.GetDateTime(2)
         //});
         var sp = new SqlSelectParser();
         var info = sp.Parse(sql);
         var factory = info.CreateBinder<Foo>();
         factory.Compile();

      }
   }
}
