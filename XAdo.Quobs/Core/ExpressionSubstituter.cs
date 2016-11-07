using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace XAdo.Quobs.Core
{
   public class ExpressionSubstituter : ExpressionVisitor
   {
      private IDictionary<MemberInfo, Expression> _substituteExpressions;

      public Expression Substitute(LambdaExpression expression, IDictionary<MemberInfo, Expression> substituteExpressions,
         ParameterExpression substituteParameter)
      {
         _substituteExpressions = substituteExpressions;
         var body = Visit(expression.Body);
         return Expression.Lambda(body, substituteParameter);
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         Expression swap;
         return _substituteExpressions.TryGetValue(node.Member, out swap) ? swap : base.VisitMember(node);
      }
   }
}