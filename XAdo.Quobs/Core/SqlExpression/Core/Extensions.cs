using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace XAdo.Quobs.Core.SqlExpression.Core
{
   public static class Extensions
   {
      #region Expression
      public static Expression Unquote(this Expression self)
      {
         while (true)
         {
            if (self == null || self.NodeType != ExpressionType.Quote) return self;
            self = ((UnaryExpression)self).Operand;
         }
      }

      public static MemberExpression GetMemberExpression(this Expression expression, bool throwException = true)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.MemberAccess:
               return expression as MemberExpression;
            case ExpressionType.Convert:
            case ExpressionType.Quote:
               return GetMemberExpression(((UnaryExpression)expression).Operand);

         }
         if (throwException)
            throw new ArgumentException("Member expression expected in: " + expression);
         return null;
      }

      public static object GetExpressionValue(this Expression expression)
      {
         object result;
         if (TryEvaluate(expression, out result))
         {
            return result;
         }
         throw new ArgumentException("Invalid node in expression: " + expression);
      }

      //public static object EvalCallExpression(this MethodCallExpression callExpression)
      //{
      //   var arguments = callExpression.Arguments.Select(GetExpressionValue).ToArray();
      //   var target = callExpression.Object != null ? GetExpressionValue(callExpression.Object) : null;
      //   return callExpression.Method.Invoke(target, arguments);
      //}

      public static bool TryEvaluate(this Expression expression, out object result)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Convert:
               var e = (UnaryExpression) expression;
               if (!e.Operand.TryEvaluate(out result))
               {
                  return false;
               }
               result = TypeDescriptor.GetConverter(e.Type).ConvertFrom(result);
               return true;
            case ExpressionType.Quote:
               return ((UnaryExpression)expression).Operand.TryEvaluate(out result);
            case ExpressionType.Constant:
               result = ((ConstantExpression)expression).Value;
               return true;
            case ExpressionType.Call:
               return expression.CastTo<MethodCallExpression>().TryEvaluateCallExpression(out result);
            case ExpressionType.MemberAccess:
               var memberExpr = (MemberExpression)expression;
               object obj = null;
               if (memberExpr.Expression != null)
               {
                  if (!TryEvaluate(memberExpr.Expression, out obj))
                  {
                     result = null;
                     return false;
                  }
               }
               result = memberExpr.Member.GetValue(obj);
               return true;
            default:
               try
               {
                  result = Expression.Lambda(expression).Compile().DynamicInvoke();
                  return true;
               }
               catch
               {
                  result = null;
                  return false;
               }
         }
         
      }

      public static bool TryEvaluateCallExpression(this MethodCallExpression callExpression, out object result)
      {
         var arguments = new object[callExpression.Arguments.Count];
         for (var i = 0; i < arguments.Length; i++)
         {
            if (!TryEvaluate(callExpression.Arguments[i], out arguments[i]))
            {
               result = null;
               return false;
            }
         }
         object target = null;
         if (callExpression.Object != null)
         {
            if (!TryEvaluate(callExpression.Object, out target))
            {
               result = null;
               return false;
            }
         }
         result = callExpression.Method.Invoke(target, arguments);
         return true;

      }
      #endregion

      #region Members
      public static Type GetMemberType(this MemberInfo member)
      {
         switch (member.MemberType)
         {
            case MemberTypes.Field:
               return ((FieldInfo)member).FieldType;
            case MemberTypes.Method:
               return ((MethodInfo)member).ReturnType;
            case MemberTypes.Property:
               return ((PropertyInfo)member).PropertyType;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }

      public static object GetValue(this MemberInfo member, object target)
      {
         var pi = member as PropertyInfo;
         return pi != null ? pi.GetValue(target) : ((FieldInfo)member).GetValue(target);
      }

      #endregion

      public static bool IsNullable(this Type self)
      {
         return self != null && Nullable.GetUnderlyingType(self) != null;
      }

      private readonly static HashSet<Type> ScalarTypes = new HashSet<Type> { typeof(string), typeof(decimal), typeof(Guid), typeof(DateTime), typeof(TimeSpan), typeof(byte[]) };
      private readonly static HashSet<string> SupportedSqlServerTypes = new HashSet<string>
        {
            "Microsoft.SqlServer.Types.SqlGeography",
            "Microsoft.SqlServer.Types.SqlHierarchyId"
        };

      public static bool IsSqlColumnType(this Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         return type.IsPrimitive || type.IsEnum || ScalarTypes.Contains(type) ||
                SupportedSqlServerTypes.Contains(type.FullName);
      }

   }
}