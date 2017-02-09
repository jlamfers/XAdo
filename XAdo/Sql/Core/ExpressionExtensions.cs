using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XAdo.Core;

namespace XAdo.Sql.Core
{
   internal static class ExpressionExtensions
   {
      public static Expression Trim(this Expression self)
      {
         if (self == null)
         {
            return null;
         }

         switch (self.NodeType)
         {
            case ExpressionType.Convert:
            case ExpressionType.Quote:
               return self.CastTo<UnaryExpression>().Operand.Trim();
         }
         return self;

      }

      public static bool IsNullConstant(this Expression self)
      {
         var constant = self.Trim() as ConstantExpression;
         return constant != null && (constant.Value == null || constant.Value == DBNull.Value);
      }

      public static bool IsParameterDependent(this MemberExpression self, StringBuilder path)
      {
         if (self == null || self.Expression == null)
         {
            return false;
         }
         if (path.Length > 0)
         {
            path.Insert(0, '.');
         }
         path.Insert(0,self.Member.Name);
         var node = self.Expression.Trim();
         return node.NodeType == ExpressionType.Parameter || IsParameterDependent(node as MemberExpression, path);
      }

      public static object GetExpressionValue(this Expression expression)
      {
         // traversing the tree performs better than compiling (generally)
         object result;
         if (TryEvaluate(expression, out result))
         {
            return result;
         }
         throw new ArgumentException("Expression cannot be evaluated: " + expression);
      }
      public static bool TryEvaluate(this Expression expression, out object result)
      {
         expression = expression.Trim();

         switch (expression.NodeType)
         {
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
               result = null;
               return false;
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
      public static object GetValue(this MemberInfo member, object target)
      {
         var pi = member as PropertyInfo;
         return pi != null ? pi.GetValue(target) : ((FieldInfo)member).GetValue(target);
      }

      public static bool IsNullable(this Type self)
      {
         return self != null && Nullable.GetUnderlyingType(self) != null;
      }

      public static SqlFormatAttribute GetSqlFormatAttribute(this MemberInfo self, Type providerType)
      {
         return self
            .GetAnnotations<SqlFormatAttribute>()
            .Where(a => a.Provider == null || providerType.IsAssignableFrom(a.Provider.GetType()))
            .OrderBy(a => CountBaseTypes(a.Provider))
            .FirstOrDefault();
      }
      public static SqlFormatAttribute GetSqlFormatAttribute(this MemberInfo self, ISqlDialect dialect)
      {
         return self.GetSqlFormatAttribute(dialect.GetType());
      }

      private static int CountBaseTypes(object instance)
      {
         if (instance == null) return int.MaxValue;
         var type = instance.GetType();
         var count = 0;
         while (type != null && type != typeof (object))
         {
            count++;
            type = type.BaseType;
         }
         return count;
      }

      public static IEnumerable<Expression> GetAllArguments(this MethodCallExpression node)
      {
         return node.Object != null ? new[] {node.Object}.Concat(node.Arguments) : node.Arguments;
      }
   }
}
