using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Core.Interface;
using XAdo.Quobs;
using XAdo.Quobs.Sql;
using XAdo.Quobs.Sql.Formatter;
using XAdo.Quobs.Generator;

namespace XAdo.UnitTest
{

   public static class AdoSessionExtension
   {
      private class SqlExecuter : ISqlExecuter
      {
         private readonly IAdoSession _session;

         public SqlExecuter(IAdoSession session)
         {
            _session = session;
         }

         public T ExecuteScalar<T>(string sql, IDictionary<string, object> args)
         {
            return _session.ExecuteScalar<T>(sql, args);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args)
         {
            return _session.Query<T>(sql, args, false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, string sqlCount, out long count)
         {
            var result = _session.QueryMultiple(sqlCount + ";" + sql, args);
            count = result.Read<long>().Single();
            return result.Read<T>(false);
         }
      }

      public static IQuob<T> From<T>(this IAdoSession self)
      {
         return new Quob<T>(new SqlServerFormatter(), new SqlExecuter(self));
      }
   }

   [TestClass]
   public class QuobTest
   {
      public class Temp
      {
         public Temp() { }
         public Temp(string _item1, decimal? _item2, int test)
         {
            this._Item1 = _item1;
            this._Item2 = _item2;
            Description = (string)TypeDescriptor.GetConverter(typeof (string)).ConvertFrom(test);
         }

         public string _Item1;// { get; set; }
         public decimal? _Item2;// { get; set; }
         public string Description;//{ get; set; }
      
      }

      [TestMethod]
      public void WorkOrdersCanBeQueriedWithProperties()
      {
         using (var session = Db.Northwind.CreateSession())
         {
            long count;
            var list = session.From<DbSalesOrderDetail>()
               .OrderBy(x => x.CarrierTrackingNumber)
               .Where(x => x.ModifiedDate != DateTime.Now && x.ProductSpecialOffer().ModifiedDate != null && x.ProductSpecialOffer().SpecialOffer().Description != "No Discount")
               //.Take(10)
               .Select(x => new { _Item1 = x.CarrierTrackingNumber, _Item2 = x.UnitPriceDiscount, Description = x.ProductSpecialOffer().SpecialOffer().Description }, out count)
               .ToList();
            //var sw = new Stopwatch();
            //sw.Start();
            //var list2 = session.From<DbSalesOrderDetail>()
            //   .OrderBy(x => x.CarrierTrackingNumber)
            //   .Where(x => x.ModifiedDate != DateTime.Now)
            //   .Take(10)
            //   .Select(x => new Temp { _Item1 = x.CarrierTrackingNumber, _Item2 = x.UnitPriceDiscount }, out count)
            //   .ToList();
            //sw.Stop();
            //Debug.WriteLine("#" + count);
            //Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [TestMethod]
      public void PerformanceTest()
      {
         using (var session = Db.Northwind.CreateSession())
         {
            List<dynamic> list = (List<dynamic>)session.Query("select * from Sales.SalesOrderDetail");
            var sw = new Stopwatch();
            sw.Start();
            list = session.Query("select * from Sales.SalesOrderDetail") as List<dynamic>;
            sw.Stop();//900
            //object s = list[0].SalesOrderID;
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      public enum FooEnum
      {
         Aap,
         Noot,
         Mies
      }
      public class Foo
      {
         public Foo(long arg)
         {
            Arg = arg;
         }
         public long Arg { get; set; }
      }

      [TestMethod]
      public void SelectTest()
      {
         //NOTE: this does not work without ctor binder, so not with member binders!!!
         //TODO: why??

         using (var session = Db.Northwind.CreateSession())
         {
           session.Query<Foo>("select 1 as Arg from Sales.SalesOrderDetail");
            var sw = new Stopwatch();
            sw.Start();
            session.Query<Foo>("select 1 as Arg from Sales.SalesOrderDetail");
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

   }
}
