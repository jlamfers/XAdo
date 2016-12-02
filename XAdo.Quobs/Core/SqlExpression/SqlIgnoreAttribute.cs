using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core.SqlExpression
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