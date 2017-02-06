using System;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.SqlExpression.Attributes
{
   public class SqlBetweenAttribute : CustomSqlExpressionBuilderAttribute
   {
      private class BetweenSqlBuilder : ICustomSqlExpressionBuilder
      {
         public void BuildSql(ExpressionVisitor parent, SqlBuilderContext context, Expression expression)
         {
            var f = context.Formatter;
            var w = context.Writer;
            var m = expression as MethodCallExpression;

            if (m == null) throw new ArgumentException("Expected a MethodCallExpression", "expression");

            w.Write("(");
            parent.Visit(m.Arguments[0]);
            w.Write(" >= ");
            parent.Visit(m.Arguments[1]);
            w.Write(" AND ");
            parent.Visit(m.Arguments[0]);
            w.Write(" <= ");
            parent.Visit(m.Arguments[2]);
            w.Write(")");
         }
      }

      public SqlBetweenAttribute()
      {
         Builder = new BetweenSqlBuilder();
      }
   }
}