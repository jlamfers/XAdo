using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace XAdo.Core
{
   public static class MemberInfoFinder
   {

      public static MemberInfo GetPropertyOrField(this Type self, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
      {
         var members = self.GetMember(name, flags | BindingFlags.IgnoreCase).Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field).ToArray();
         return members.Length > 1 
            ? self.GetMember(name, flags).SingleOrDefault() 
            : members.SingleOrDefault();
      }
      public static MemberInfo[] GetPropertiesAndFields(this Type self, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
      {
         return self.GetMembers(flags)
               .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field)
               .ToArray();
      }
      public static Type GetMemberType(this MemberInfo self)
      {
         switch (self.MemberType)
         {
            case MemberTypes.Field:
               return ((FieldInfo)self).FieldType;
            case MemberTypes.Property:
               return ((PropertyInfo)self).PropertyType;
            default:
               throw new ArgumentOutOfRangeException("self", "Only Field and Properties are supported");
         }
      }
      public static object GetValue(this MemberInfo member, object target)
      {
         var pi = member as PropertyInfo;
         return pi != null ? pi.GetValue(target) : ((FieldInfo)member).GetValue(target);
      }


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

      public static MemberInfo GetMemberInfo(this Expression expression, bool throwException = true)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Lambda:
               return GetMemberInfo(((LambdaExpression)expression).Body);
            case ExpressionType.Convert:
            case ExpressionType.Quote:
               return GetMemberInfo(((UnaryExpression)expression).Operand);
            case ExpressionType.MemberAccess:
               return ((MemberExpression)expression).Member;
            case ExpressionType.Call:
               return ((MethodCallExpression)expression).Method;
            default:
               if (throwException)
                  throw new ArgumentException(String.Format("Cannot obtain a MemberExpression from '{0}' ", expression), "expression");
               return null;
         }
      }
   }
}