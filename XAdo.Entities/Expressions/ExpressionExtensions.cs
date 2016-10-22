using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Attributes;

namespace XAdo.Quobs.Expressions
{
   internal static class ExpressionExtensions
   {

      public static bool IsJoinMethod(this Expression expression)
      {
         var callExpression = expression as MethodCallExpression;
         return callExpression != null && callExpression.Method.GetAnnotation<JoinMethodAttribute>() != null;
      }

      public static bool IsParameter(this Expression expression)
      {
         return expression != null && expression.NodeType == ExpressionType.Parameter;
      }

      public static bool IsParameterMember(this Expression expression)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Convert:
               return IsParameterMember(((UnaryExpression)expression).Operand);
            case ExpressionType.MemberAccess:
               return IsParameterMember(((MemberExpression)expression).Expression);
            case ExpressionType.Parameter:
               return true;
            default:
               return false;
         }
      }

      public static Expression TrimConvert(this Expression expression)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Convert:
               return TrimConvert(((UnaryExpression)expression).Operand);
            default:
               return expression;
         }
      }

      public static object GetExpressionValue(this Expression expression)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Convert:
               return GetExpressionValue(((UnaryExpression) expression).Operand);
            case ExpressionType.Constant:
               return ((ConstantExpression) expression).Value;
            case ExpressionType.Call:
               return EvalCallExpression(expression as MethodCallExpression);
            case ExpressionType.MemberAccess:
               var memberExpr = (MemberExpression) expression;
               var obj = memberExpr.Expression == null ? null : GetExpressionValue(memberExpr.Expression);
               var pi = memberExpr.Member as PropertyInfo;
               return pi != null ? pi.GetValue(obj) : ((FieldInfo) (memberExpr.Member)).GetValue(obj);
            default:
               throw new ArgumentException(expression+": expected constant/call/memberaccess expression");
         }
      }

      public static object EvalCallExpression(this MethodCallExpression callExpression)
      {
         var arguments = callExpression.Arguments.Select(GetExpressionValue).ToArray();
         var target = callExpression.Object != null ? GetExpressionValue(callExpression.Object) : null;
         return callExpression.Method.Invoke(target, arguments);
      }

   }
}
