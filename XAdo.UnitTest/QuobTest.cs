using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Core.Interface;
using XAdo.Quobs;
using XAdo.Quobs.Attributes;
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
         public IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, out long count)
         {
            var reader = _session.QueryMultiple(sql, args);
            count = reader.Read<long>().Single();
            return reader.Read<T>(false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, string sqlCount, out long count)
         {
            var result = _session.QueryMultiple(sqlCount + ";" + sql, args);
            count = result.Read<long>().Single();
            return result.Read<T>(false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory, IDictionary<string, object> args)
         {
            return _session.Query<T>(sql, factory, args, false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory, IDictionary<string, object> args, string sqlCount, out long count)
         {
            count = _session.ExecuteScalar<long>(sqlCount);
            return _session.Query<T>(sql, factory, args, false);
         }
      }

      public static Quob<T> From<T>(this IAdoSession self)
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
            Description = (string)TypeDescriptor.GetConverter(typeof(string)).ConvertFrom(test);
         }

         public static Temp CreateTemp(IDataRecord r, Delegate[] d)
         {
            return new Temp(((Func<IDataRecord, int, string>)d[0])(r, 11), ((Func<IDataRecord, int, decimal?>)d[1])(r, 12), ((Func<IDataRecord, int, int>)d[2])(r, 13));
         }

         public static void DefaultTest()
         {
            var s = default(string);
            var i = default(int);
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
               .Take(10)
               .Select(x => new { _Item1 = x.CarrierTrackingNumber, _Item2 = x.UnitPriceDiscount, Description = x.ProductSpecialOffer().SpecialOffer().Description })//, out count)
               .Where(i => i._Item2 > 0)
               .OrderBy(i => i._Item1)
               .ToList();
            var sw = new Stopwatch();
            sw.Start();
            var q = session.From<DbSalesOrderDetail>()
               .OrderBy(x => x.CarrierTrackingNumber)
               .Where(x => x.ModifiedDate != DateTime.Now)
               .Take(10)
               .Skip(10)
               .Select(x => new { _Item1 = x.CarrierTrackingNumber, _Item2 = x.UnitPriceDiscount })//, out count)
               .AddOrderBy(x => x._Item2)
               ;
            var sql = q.CastTo<ISqlBuilder>().GetSql();
            var list2 = q.ToList();
               
            sw.Stop();
            //Debug.WriteLine("#" + count);
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [TestMethod]
      public void QuobWorks()
      {
         using (var session = Db.Northwind.CreateSession())
         {
            long count;
            var list = session
               .From<DbPerson>()
               .Skip(10)
               .Take(10)
               .OrderBy(p => p.FirstName)
               .ToList(out count);
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

      [TestMethod]
      public void Test123()
      {
         Debug.WriteLine(typeof(int?).IsValueType);
      }

      public enum FooEnum
      {
         Aap,
         Noot,
         Mies
      }
      public class Foo
      {
         public Foo() { }
         public Foo(int arg, DateTime dt)
         {
            Arg = arg;
            Dt = dt;
         }

         public int Arg;//{ get; set; }
         public DateTime Dt { get; set; }
      }

      [Table("SalesOrderDetail", Schema = "Sales")]
      public partial class _DbSalesOrderDetail
      {
         public _DbSalesOrderDetail() { }
         public _DbSalesOrderDetail(int salesOrderId, int salesOrderDetailId, short orderQty, int productId, int specialOfferId, decimal unitPrice, decimal unitPriceDiscount, decimal lineTotal, Guid rowguid, DateTime modifiedDate)
         {
            ModifiedDate = modifiedDate;
            this.rowguid = rowguid;
            LineTotal = lineTotal;
            UnitPriceDiscount = unitPriceDiscount;
            UnitPrice = unitPrice;
            SpecialOfferID = specialOfferId;
            ProductID = productId;
            OrderQty = orderQty;
            SalesOrderDetailID = salesOrderDetailId;
            SalesOrderID = salesOrderId;
         }

         public virtual Int32 SalesOrderID { get; set; }
         public virtual Int32 SalesOrderDetailID { get; set; }
         //public virtual String CarrierTrackingNumber { get; private set; }
         public virtual Int16 OrderQty { get; set; }
         public virtual Int32 ProductID { get; set; }
         public virtual Int32 SpecialOfferID { get; set; }
         public virtual Decimal UnitPrice { get; set; }
         public virtual Decimal UnitPriceDiscount { get; set; }
         public virtual Decimal LineTotal { get; set; }
         public virtual Guid rowguid { get; set; }
         public virtual DateTime ModifiedDate { get; set; }
      }


      [TestMethod]
      public void SelectTest()
      {
         //370 ms
         //NOTE: this does not work without ctor binder, so not with member binders!!!
         //TODO: why??

         //using (var session = Db.Northwind.CreateSession())
         //{
         //  var list = session.Query<Foo>("select 1 as Arg from Sales.SalesOrderDetail");
         //   var sw = new Stopwatch();
         //   sw.Start();
         //   session.Query<Foo>("select 1 as Arg from Sales.SalesOrderDetail");
         //   sw.Stop();
         //   Debug.WriteLine(sw.ElapsedMilliseconds);
         //}
         using (var session = Db.Northwind.CreateSession())
         {
            var list = session.Query<DbSalesOrderDetail>("select * from Sales.SalesOrderDetail");
            var sw = new Stopwatch();
            sw.Start();
            session.Query<DbSalesOrderDetail>("select * from Sales.SalesOrderDetail");
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

      [TestMethod]
      public void SelectTest2()
      {
         using (var session = Db.Northwind.CreateSession())
         {
            var list = session.Query<DbDocument>("select * from Production.Document");
            var sw = new Stopwatch();
            sw.Start();
            list = session.Query<DbDocument>("select * from Production.Document");
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }

   }
}
