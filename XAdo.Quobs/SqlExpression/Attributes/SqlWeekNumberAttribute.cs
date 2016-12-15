using System;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.SqlExpression.Attributes
{
   public class SqlWeekNumberAttribute : CustomSqlExpressionBuilderAttribute
   {
      private class WeekNrSqlBuilder : ICustomSqlExpressionBuilder
      {
         public void BuildSql(ExpressionVisitor parent, SqlBuilderContext context, Expression expression)
         {
            var f = context.Formatter;
            var w = context.Writer;
            var m = expression as MethodCallExpression;

            if (m == null) throw new ArgumentException("Expected a MethodCallExpression","expression");

            f.WriteDateTimeWeekNumber(context.Writer, w2 =>
            {
               context.Writer = w2 ?? w;
               parent.Visit(m.Arguments[0]);
               context.Writer = w;
            });
         }
      }

      public SqlWeekNumberAttribute()
      {
         Builder = new WeekNrSqlBuilder();
      }
   }
}