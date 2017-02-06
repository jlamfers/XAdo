using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.SqlObjects.Dialects;

namespace XAdo.SqlObjects.SqlExpression.Attributes
{
   public class SqlInAttribute : CustomSqlExpressionBuilderAttribute
   {
      private static readonly ConcurrentDictionary<MemberInfo,int> 
         _ids = new ConcurrentDictionary<MemberInfo, int>();

      private class InSqlBuilder : ICustomSqlExpressionBuilder
      {
         public void BuildSql(ExpressionVisitor parent, SqlBuilderContext context, Expression expression)
         {
            var f = context.Formatter;
            var w = context.Writer;
            var m = expression as MethodCallExpression;

            if (m == null) throw new ArgumentException("Expected a MethodCallExpression", "expression");

            parent.Visit(m.Arguments[0]);
            w.Write(" IN (");
            var args = m.Arguments[1] as NewArrayExpression;
            if (args != null)
            {
               var comma = "";
               foreach (var e in args.Expressions)
               {
                  w.Write(comma);
                  parent.Visit(e);
                  comma = ", ";
               }
            }
            else
            {
               object result;
               if (!m.Arguments[1].TryEvaluate(out result))
               {
                  throw new SqlObjectsException("Invalid argument type for operator IN: "+m.Arguments[1].Type);
               }
               var member = m.Arguments[0].GetMemberInfo();
               var list = (IEnumerable) result;
               var comma = "";
               var parName = context.Aliases.InParameter(_ids.GetOrAdd(member, x => _ids.Count + 1));
               var index = 0;
               foreach (var v in list)
               {
                  w.Write(comma);
                  var p = parName + (index++);
                  w.Write(f.FormatParameter(p));
                  context.Arguments.Add(p,v);
                  comma = ", ";
               }
            }
            w.Write(")");

         }
      }

      public SqlInAttribute()
      {
         Builder = new InSqlBuilder();
      }
   }
}