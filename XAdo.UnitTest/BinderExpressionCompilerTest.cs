using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Quobs;
using XAdo.Quobs.Attributes;
using XAdo.Quobs.Expressions;
using XAdo.Quobs.Generator;
using XAdo.Quobs.Sql;

namespace XAdo.UnitTest
{
   [TestClass]
   public class BinderExpressionCompilerTest
   {

      public class QuobMock<T>
      {
         public static BinderExpressionCompiler.CompileResult Compile<TMapped>(Expression<Func<T, TMapped>> expression)
         {
            var c = new BinderExpressionCompiler();
            var result = c.Compile(expression);
            return result;
         }
      }

      public class Person
      {
         public string Name { get; set; }
         public string Address { get; set; }
      }

      public class Being
      {
         public string Name { get; set; }
         public string Address { get; set; }
      }

      [TestMethod]
      public void Test()
      {
         var result = QuobMock<DbCustomer>.Compile(p => new {Name = p.Person().FirstName, p.Person().LastName, p.AccountNumber, Address=new{p.Person().BusinessEntity().BusinessEntityID}});
         Debug.WriteLine(result.BinderExpression);
         var d = result.BinderExpression.Compile();
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            var r = QuobMock<DbCustomer>.Compile(p => new { Name = p.Person().FirstName, p.Person().LastName, p.AccountNumber, Address = new { p.Person().BusinessEntity().BusinessEntityID } });
            //result.BinderExpression.Compile();
            r.BinderExpression.ToString();
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }

      [TestMethod]
      public void Test2()
      {
         Stopwatch sw;
         using (var sn = Db.Northwind.CreateSession())
         {
            var quob = sn.From<DbCustomer>();
            var q = quob
               .Select(
                  p =>
                     new
                     {
                        Name = p.Person().FirstName,
                        p.Person().LastName,
                        p.AccountNumber,
                        Address = new
                        {
                           p.Person().BusinessEntity().BusinessEntityID
                        }
                     });
            var sql = quob.CastTo<ISqlBuilder>().GetSql();
            q.ToList();
            sn.Query(sql);
            sw = new Stopwatch();
            sw.Start();
            sn
                           .From<DbCustomer>()
                           .Select(
                              p =>
                                 new
                                 {
                                    Name = p.Person().FirstName,
                                    p.Person().LastName,
                                    p.AccountNumber,
                                    Address = p.Person().BusinessEntity(JoinType.Left).BusinessEntityID != null ? new
                                    {
                                       Id = p.Person().BusinessEntity().BusinessEntityID ?? 10
                                    } : null
                                 })
                                 .ToList();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
            sw = new Stopwatch();
            sw.Start();
            sn.Query(sql);
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }
      }


   }
}
