using System;

namespace XAdo.SqlObjects.SqlExpression.Attributes
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
