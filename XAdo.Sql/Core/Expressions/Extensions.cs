using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Parser;

namespace XAdo.Quobs.Linq
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
         var arguments = node.Arguments.ToList();
         var hasParams = node.Method.GetParameters().Any() && node.Method.GetParameters().Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
         if (hasParams)
         {
            var last = arguments.Last();
            arguments.RemoveAt(arguments.Count - 1);
            arguments.AddRange(last.CastTo<NewArrayExpression>().Expressions);
         }

         var argsEnumerable = node.Object != null ? new[] { node.Object }.Concat(arguments) : arguments;

         if (!node.Method.IsGenericMethod)
         {
            return argsEnumerable;
         }
         var att = node.Method.GetAnnotations<SqlFormatAttribute>().OrderBy(f => f.Order).FirstOrDefault();
         if (att == null || !att.IncludeGenericParameters)
         {
            return argsEnumerable;
         }
         var args = argsEnumerable.ToList();
         args.AddRange(node.Method.GetGenericArguments().Select(Expression.Constant));
         return args;
      }

      public static IDictionary<MemberInfo, string> GetMemberToFullNameMap(this Type type, IDictionary<MemberInfo, string> map = null, string path = null)
      {
         map = map ?? new Dictionary<MemberInfo, string>();
         path = path ?? "";
         foreach (var m in type.GetPropertiesAndFields())
         {
            if (map.ContainsKey(m)) return map;
            var p = (path + Constants.Syntax.Chars.NAME_SEP_STR + m.Name).TrimStart(Constants.Syntax.Chars.NAME_SEP);
            var t = m.GetMemberType();
            if (!t.IsScalarType())
            {
               t.GetMemberToFullNameMap(map, p);
            }
            //map[m] = p;
            else
            {
               map[m] = p;
            }
         }
         return map;

      }

      public static IDictionary<string, MemberInfo> GetFullNameToMemberMap(this Type type)
      {
         var dict = type.GetMemberToFullNameMap().ToDictionary(m => m.Value, m => m.Key, StringComparer.OrdinalIgnoreCase);
         return dict;
      }


   }
}
