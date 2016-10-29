using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace XAdo.Quobs.Expressions
{
   public class ExpressionSwapper : ExpressionVisitor
   {
      private IDictionary<MemberInfo, Expression> _swapExpressions;

      public Expression Substitute(LambdaExpression expression, IDictionary<MemberInfo, Expression> swapExpressions,
         ParameterExpression swapParameter)
      {
         _swapExpressions = swapExpressions;
         var body = Visit(expression.Body);
         return Expression.Lambda(body, swapParameter);
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         Expression swap;
         return _swapExpressions.TryGetValue(node.Member, out swap) ? swap : base.VisitMember(node);
      }
   }
}