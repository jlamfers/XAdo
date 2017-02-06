using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace XAdo.SqlObjects.SqlExpression.Visitors
{
   class StrongBoxVisitor : ExpressionVisitor
   {
      private readonly ParameterExpression _parameter;
      private readonly Type _strongBoxType;

      public StrongBoxVisitor(ParameterExpression parameter)
      {
         _parameter = parameter;
         _strongBoxType = typeof (StrongBox<>).MakeGenericType(_parameter.Type);

      }

      protected override Expression VisitMember(MemberExpression node)
      {
         return node.Member.DeclaringType == _strongBoxType ? _parameter : base.VisitMember(node);
      }
   }
}
