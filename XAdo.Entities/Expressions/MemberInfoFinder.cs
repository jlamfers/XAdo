using System;
using System.Linq.Expressions;
using System.Reflection;

namespace XAdo.Quobs.Expressions
{
   public static class MemberInfoFinder
   {

      public static MethodInfo GetMethodInfo(this Expression<Action> expression)
      {
         return (MethodInfo)GetMemberInfo(expression);
      }
      public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> expression)
      {
         return (MethodInfo)GetMemberInfo(expression);
      }
      public static MethodInfo GetMethodInfo<T, TResult>(this Expression<Func<T, TResult>> expression)
      {
         return (MethodInfo)GetMemberInfo(expression);
      }

      public static PropertyInfo GetPropertyInfo(this Expression<Func<object>> expression)
      {
         return (PropertyInfo)expression.GetMemberInfo();
      }
      public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> expression)
      {
         return (PropertyInfo)expression.GetMemberInfo();
      }

      public static FieldInfo GetFieldInfo(this Expression<Func<object>> expression)
      {
         return (FieldInfo)expression.GetMemberInfo();
      }
      public static FieldInfo GetFieldInfo<T>(this Expression<Func<T, object>> expression)
      {
         return (FieldInfo)expression.GetMemberInfo();
      }


      public static MemberInfo GetMemberInfo(this Expression expression)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Lambda:
               return GetMemberInfo(((LambdaExpression)expression).Body);
            case ExpressionType.Convert:
               return GetMemberInfo(((UnaryExpression)expression).Operand);
            case ExpressionType.MemberAccess:
               return ((MemberExpression)expression).Member;
            case ExpressionType.Call:
               return ((MethodCallExpression)expression).Method;
            default:
               throw new ArgumentException(string.Format("Cannot obtain a MemberExpression from '{0}' ", expression), "expression");
         }
      }



   }
}
