using System;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.SqlExpression.Attributes
{
   public class SqlIgnoreAttribute : CustomSqlExpressionBuilderAttribute
   {
      private class IgnoreSqlBuilder : ICustomSqlExpressionBuilder
      {
         public void BuildSql(ExpressionVisitor parent, SqlBuilderContext context, Expression expression)
         {
            var m = expression as MethodCallExpression;
            if (m == null) throw new ArgumentException("Expected a MethodCallExpression", "expression");
            parent.Visit(m.Arguments[0]);
         }
      }

      public SqlIgnoreAttribute()
      {
         Builder = new IgnoreSqlBuilder();
      }
   }
}