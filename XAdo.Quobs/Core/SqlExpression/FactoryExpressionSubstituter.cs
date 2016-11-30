using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.SqlExpression.Core;

namespace XAdo.Quobs.Core.SqlExpression
{
   internal class FactoryExpressionSubstituter : ExpressionVisitor
   {
      private ParameterExpression _newParameter;
      private Type _joinClassType;
      private ParameterExpression _parameter;
      private Expression _substitute;
      private bool _registerFirstMember = true;

      public MemberExpression FirstMember { get; private set; }

      public Expression Substitute(LambdaExpression expression, ParameterExpression parameter, Expression substitute, ParameterExpression newParameter)
      {
         _parameter = parameter;
         _substitute = substitute;
         _newParameter = newParameter;
         _joinClassType = expression.Parameters[0].Type;
         return Visit(expression);
      }

      protected override Expression VisitLambda<T>(Expression<T> node)
      {
         if (!_registerFirstMember)
         {
            return Visit(node.Body);
         }
         return Expression.Lambda(Visit(node.Body));
      }


      protected override Expression VisitParameter(ParameterExpression node)
      {
         return node == _parameter ? _substitute : base.VisitParameter(node);
      }


      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         Expression substitute;
         return TrySubstituteFactoryMethod(node, _newParameter, out substitute) 
            ? Visit(substitute) 
            : base.VisitMethodCall(node);
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         if (_registerFirstMember && FirstMember == null && node.Expression != null && node.Expression.GetParameterType() == _joinClassType)
         {
            FirstMember = Expression.MakeMemberAccess(Visit(node.Expression),node.Member);
         }
         return base.VisitMember(node);
      }

      public bool TrySubstituteFactoryMethod(MethodCallExpression node, ParameterExpression newParameter, out Expression expression)
      {
         expression = null;
         if (node.Method.EqualMethods(KnownMembers.SqlMethods.DefaultIfEmpty))
         {
            var newExpression = (LambdaExpression) node.Arguments[1];
            var substituter = new FactoryExpressionSubstituter();
            var arg2 = substituter.Substitute(newExpression, newExpression.Parameters[0], node.Arguments[0],newParameter);
            var arg1 = Expression.Convert(substituter.FirstMember, typeof (object));
            var m = KnownMembers.SqlMethods.DefaultIfEmpty2.MakeGenericMethod(node.Method.GetGenericArguments()[1]);
            expression = Expression.Call(m, arg1, arg2);
         }
         else if (node.Method.EqualMethods(KnownMembers.SqlMethods.Create))
         {
            _registerFirstMember = false;
            var newExpression = (LambdaExpression)node.Arguments[1];
            var substituter = new FactoryExpressionSubstituter();
            expression = substituter.Substitute(newExpression, newExpression.Parameters[0], node.Arguments[0], newParameter);
         }
         return expression != null;
      }

   }
}
