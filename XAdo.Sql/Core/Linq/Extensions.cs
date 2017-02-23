using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using XAdo.Sql.Core.Common;

namespace XAdo.Sql.Core.Linq
{
   public static class Extensions
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
      public static Expression Convert(this Expression self, Type type)
      {
         return (self.Type == type ? self : Expression.Convert(self, type));
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
         path.Insert(0, self.Member.Name);
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

      public static IEnumerable<Expression> GetAllArguments(this MethodCallExpression node)
      {
         var argsEnumerable = node.Object != null ? new[] { node.Object }.Concat(node.Arguments) : node.Arguments;

         if (!node.Method.IsGenericMethod)
         {
            return argsEnumerable;
         }
         var att = node.Method.GetAnnotation<SqlFormatAttribute>();
         if (att == null || !att.IncludeGenericParameters)
         {
            return argsEnumerable;
         }
         var args = argsEnumerable.ToList();
         args.AddRange(node.Method.GetGenericArguments().Select(Expression.Constant));
         return args;
      }

   }
}
