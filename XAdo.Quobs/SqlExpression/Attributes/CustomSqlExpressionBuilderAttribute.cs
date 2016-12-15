using System;

namespace XAdo.Quobs.Core.SqlExpression.Sql
{
   public class CustomSqlExpressionBuilderAttribute : Attribute
   {
      protected CustomSqlExpressionBuilderAttribute(){}
      public CustomSqlExpressionBuilderAttribute(Type customSqlExpressionBuilder)
      {
         Builder = Activator.CreateInstance(customSqlExpressionBuilder).CastTo<ICustomSqlExpressionBuilder>();
      }

      public ICustomSqlExpressionBuilder Builder { get; protected set; }
   }

}
