using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Quobs;
using XAdo.Quobs.Generator;
using XAdo.Quobs.Schema;
using XAdo.UnitTest.Model;

namespace XAdo.UnitTest
{
    [TestClass]
    public class XAdoTests
    {
        //TODO....

        [TestMethod]
        public void WorkOrdersCanBeQueriedDynamically()
        {
            using (var session = Db.Northwind.CreateSession())
            {
                var list = session.Query("SELECT * FROM [Production].[WorkOrder]");
                var sw = new Stopwatch();
                sw.Start();
                list = session.Query("SELECT * FROM [Production].[WorkOrder]");
                sw.Stop();
                Debug.WriteLine("#rows fetched: "+list.Count()+", elapsed: " + sw.ElapsedMilliseconds+" ms.");
            }
        }

        [TestMethod]
        public void WorkOrdersCanBeQueriedWithFields()
        {
            using (var session = Db.Northwind.CreateSession())
            {
                var list = session.Query<WorkOrderWithFields>("SELECT * FROM [Production].[WorkOrder]");
                var sw = new Stopwatch();
                sw.Start();
                list = session.Query<WorkOrderWithFields>("SELECT * FROM [Production].[WorkOrder]");
                sw.Stop();
                Debug.WriteLine("#rows fetched: " + list.Count() + ", elapsed: " + sw.ElapsedMilliseconds + " ms.");
            }
        }

        [TestMethod]
        public void WorkOrdersCanBeQueriedWithProperties()
        {
            using (var session = Db.Northwind.CreateSession().BeginTransaction())
            {
                var list = session.Query<WorkOrder>("SELECT * FROM [Production].[WorkOrder]");
                var sw = new Stopwatch();
                sw.Start();
                list = session.Query<WorkOrder>("SELECT * FROM [Production].[WorkOrder]");
                sw.Stop();
                Debug.WriteLine("#rows fetched: " + list.Count() + ", elapsed: " + sw.ElapsedMilliseconds + " ms.");
            }
        }

       [TestMethod]
       public void SqlSelectWorks()
       {
          using (var session = Db.Northwind.CreateSession())
          {
             //var list = session.Select<WorkOrder>();
          }
       }
       [TestMethod]
       public void ExpressionWorks()
       {
          //var yep = new {Id = 11, Name="oops"};
          //var list = new List<WorkOrder>(new []{new WorkOrder {TempName = "Foo"}});
          //list.Where(s => s.TempName.CompareTo("Foo") < -1).ToList();

          //var exp = GetExpression<WorkOrder>(w => w.DueDate != w.EndDate && (w.ProductID == 10 || w.ProductID == yep.Id) && w.DueDate > DateTime.UtcNow &&  w.TempName.StartsWith(w.TempName) &&  string.Compare(w.TempName,"oops") < -1);
          //var c = new SqlExpressionCompiler();
          //var result = c.Compile(exp);
       }

       public Expression GetExpression<T>(Expression<Func<T,bool>> expression)
       {
          return expression;
       }

       [TestMethod]
       public void T4Works()
       {
          new QuobsEntityCSharpGenerator().Generate(
             factory: DbProviderFactories.GetFactory("System.Data.SqlClient"),
             connectionString: @"Server=.\SqlExpress;Database=AdventureWorks2012;Trusted_Connection=true",
             @namespace: "MyApp.Database.Tables",
             classNamePrefix: "Db",
             normalizePrimaryKey: false);

       }

       [TestMethod]
       public void DbReaderWorks()
       {
          //var db = new DbSchemaReader().Read(@"Server=.\SqlExpress;Database=AdventureWorks2012;Trusted_Connection=true",
          //   "System.Data.SqlClient");

          var g = new CSharpGenerator();
          var r = g.Generate(@"Server=.\SqlExpress;Database=AdventureWorks2012;Trusted_Connection=true", "System.Data.SqlClient","Quobs.Entities");
       }

      public static class Converter<T>
      {
        public static TypeConverter Instance = TypeDescriptor.GetConverter(typeof (T));
      }

      [TestMethod]
      public void TestConvert()
      {
        TypeDescriptor.GetConverter(typeof (Guid));
        var sw = new Stopwatch();
          sw.Start();
        for (var i = 0; i < 100000; i++)
        {
          TypeDescriptor.GetConverter(typeof(Guid));
        }
        sw.Stop();
        Debug.WriteLine(sw.ElapsedMilliseconds);
        Converter<Guid>.Instance.GetType();
        sw = new Stopwatch();
        sw.Start();
        for (var i = 0; i < 100000; i++)
        {
          //var c = (Guid)Converter<Guid>.Instance.ConvertFrom("{8ACAC291-510A-42FB-A167-634E8B5D3A6A}");
          var c = new Guid("{8ACAC291-510A-42FB-A167-634E8B5D3A6A}");
        }
        sw.Stop();
        Debug.WriteLine(sw.ElapsedMilliseconds);
      }
    }
}
