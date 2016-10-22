using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Quobs.Expressions;
using XAdo.Quobs.Generator;

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

      }

      
   }
}
