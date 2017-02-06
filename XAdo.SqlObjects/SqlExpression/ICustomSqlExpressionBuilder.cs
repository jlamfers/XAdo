using System.Linq.Expressions;

namespace XAdo.SqlObjects.SqlExpression
{
   public interface ICustomSqlExpressionBuilder
   {
      void BuildSql(ExpressionVisitor visitor, SqlBuilderContext context, Expression expression);
   }
}