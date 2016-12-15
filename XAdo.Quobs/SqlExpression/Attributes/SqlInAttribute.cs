using System;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.SqlExpression.Attributes
{
   public class SqlInAttribute : CustomSqlExpressionBuilderAttribute
   {
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
            if (args == null)
            {
               throw new SqlObjectsException("Expected a NewArrayExpression in expression: " + expression + ", found: " + m.Arguments[1]);
            }
            var comma = "";
            foreach (var e in args.Expressions)
            {
               w.Write(comma);
               parent.Visit(e);
               comma = ", ";
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