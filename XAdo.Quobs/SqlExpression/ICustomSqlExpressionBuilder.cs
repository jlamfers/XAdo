using System.Linq.Expressions;

namespace XAdo.Quobs.Core.SqlExpression.Sql
{
   public interface ICustomSqlExpressionBuilder
   {
      void BuildSql(ExpressionVisitor visitor, SqlBuilderContext context, Expression expression);
   }
}