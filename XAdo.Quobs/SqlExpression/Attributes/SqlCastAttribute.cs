using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core.SqlExpression
{
   public class SqlCastAttribute : CustomSqlExpressionBuilderAttribute
   {
      private class CastSqlBuilder : ICustomSqlExpressionBuilder
      {
         public void BuildSql(ExpressionVisitor parent, SqlBuilderContext context, Expression expression)
         {
            var f = context.Formatter;
            var w = context.Writer;
            var m = expression as MethodCallExpression;

            if (m == null) throw new ArgumentException("Expected a MethodCallExpression", "expression");

            var type = m.Type;
            var target = m.Arguments[0];

            f.WriteTypeCast(w, type, w2 =>
            {
               context.Writer = w2 ?? w;
               parent.Visit(target);
               context.Writer = w;
            });
         }
      }

      public SqlCastAttribute()
      {
         Builder = new CastSqlBuilder();
      }
   }
}