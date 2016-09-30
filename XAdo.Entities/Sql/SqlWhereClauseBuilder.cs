using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XAdo.Entities.Expressions;
using XAdo.Entities.Sql.Formatter;

namespace XAdo.Entities.Sql
{
   public class SqlWhereClauseBuilder : ExpressionVisitor
   {
      private readonly ISqlFormatter _formatter;

      public static class KnownMembers
      {
         public static class String
         {
            public static readonly MethodInfo
               StartsWith = MemberInfoFinder.GetMethodInfo<string>(s => s.StartsWith("")),
               EndsWith = MemberInfoFinder.GetMethodInfo<string>(s => s.EndsWith("")),
               Contains = MemberInfoFinder.GetMethodInfo<string>(s => s.Contains("")),
               Compare = MemberInfoFinder.GetMethodInfo<string>(s => s.CompareTo("")),
               Compare2 = MemberInfoFinder.GetMethodInfo<bool>(s => string.Compare("", ""));
         }

         public static HashSet<MethodInfo> LikeMethods = new HashSet<MethodInfo>(new[]
         {
            String.Contains,String.EndsWith,String.StartsWith
         });
         public static HashSet<MethodInfo> CompareMethods = new HashSet<MethodInfo>(new[]
         {
            String.Compare,String.Compare2
         });

      }

      public SqlWhereClauseBuilder(ISqlFormatter formatter = null)
      {
         _formatter = formatter ?? new SqlFormatter();
      }

      private StringBuilder
         _sb;

      private Dictionary<string, object>
         _arguments;

      public Tuple<string,IDictionary<string,object>> Compile(Expression expression)
      {
         _sb = new StringBuilder();
         _arguments = new Dictionary<string, object>();
         Visit(expression);
         return new Tuple<string, IDictionary<string, object>>(_sb.ToString(),_arguments);
      }

      protected override Expression VisitUnary(UnaryExpression node)
      {
         switch (node.NodeType)
         {
            case ExpressionType.Not:
               _sb.Append(" NOT ");
               Visit(node.Operand);
               break;
            case ExpressionType.Convert:
               Visit(node.Operand);
               break;
            default:
               throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
         }
         return node;
      }
      protected override Expression VisitBinary(BinaryExpression node)
      {
         if (TryHandleStringCompare(node))
         {
            // e.g., string.Compare(s1,s2) > 0
            return node;
         }

         if (node.NodeType == ExpressionType.OrElse)
         {
            _sb.Append("(");
         }
         Visit(node.Left);

         switch (node.NodeType)
         {
            case ExpressionType.AndAlso:
               _sb.Append(" AND ");
               break;

            case ExpressionType.OrElse:
               _sb.Append(" OR ");
               break;

            case ExpressionType.Equal:
               _sb.Append(IsNullConstant(node.Right) ? " IS " : " = ");
               break;

            case ExpressionType.NotEqual:
               _sb.Append(IsNullConstant(node.Right) ? " IS NOT " : " <> ");
               break;

            case ExpressionType.LessThan:
               _sb.Append(" < ");
               break;

            case ExpressionType.LessThanOrEqual:
               _sb.Append(" <= ");
               break;

            case ExpressionType.GreaterThan:
               _sb.Append(" > ");
               break;

            case ExpressionType.GreaterThanOrEqual:
               _sb.Append(" >= ");
               break;

            default:
               throw new NotSupportedException(string.Format("Expression not supported: {0}", node));

         }

         Visit(node.Right);
         if (node.NodeType == ExpressionType.OrElse)
         {
            _sb.Append(")");
         }
         return node;
      }
      protected override Expression VisitMember(MemberExpression node)
      {
         if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
         {
            _sb.Append(_formatter.FormatColumnName(node.Member));
            return node;
         }
         RegisterArgument(node.GetExpressionValue());
         return node;
      }
      protected override Expression VisitConstant(ConstantExpression node)
      {
         if (node.Value == null)
         {
            _sb.Append("NULL");
         }
         else
         {
            RegisterArgument(node.Value);
         }
         return node;
      }
      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         var m = node.Method;

         if (KnownMembers.LikeMethods.Contains(m))
         {
            Visit(node.Object);
            _sb.Append(" LIKE ");
            if (node.Arguments[0].IsParameterMember())
            {
               const string methodName = "CONCAT";
               var w = new StringWriter(_sb);
               // we cannot evaluate values if the operand is a parameter member, so we need a column reference
               if (m == KnownMembers.String.StartsWith)
               {
                  _formatter.FormatSqlMethod(methodName,w, x => Visit(node.Arguments[0]), x => x.Write("'%'"));
               }
               else if (m == KnownMembers.String.EndsWith)
               {
                  _formatter.FormatSqlMethod(methodName, w, x => x.Write("'%'"), x => Visit(node.Arguments[0]));
               }
               else if (m == KnownMembers.String.Contains)
               {
                  _formatter.FormatSqlMethod(methodName, w, x => x.Write("'%'"), x => Visit(node.Arguments[0]), x => x.Write("'%'"));
               }               
            }
            else
            {
               if (m == KnownMembers.String.StartsWith)
               {
                  RegisterArgument(node.Arguments[0].GetExpressionValue() + "%");
               }
               else if (m == KnownMembers.String.EndsWith)
               {
                  RegisterArgument("%" + node.Arguments[0].GetExpressionValue());
               }
               else if (m == KnownMembers.String.Contains)
               {
                  RegisterArgument("%" + node.Arguments[0].GetExpressionValue() + "%");
               }               
            }
         }
         else
         {
            RegisterArgument(node);
         }

         return node;
      }

      private void RegisterArgument(object value)
      {
         var argumentName = "arg_" + _arguments.Count;
         _sb.AppendFormat(_formatter.FormatParameterName(argumentName));
         _arguments[argumentName] = value;
      }

      protected bool IsNullConstant(Expression exp)
      {
         return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
      }

      private bool TryHandleStringCompare(BinaryExpression node)
      {

         MethodCallExpression left, right;
         if (!IsStringCompare(node, out left, out right))
         {
            return false;
         }

         var @operator = node.NodeType;
         MethodCallExpression compareMethod;
         int compareValue;
         Expression arg1, arg2;
         const string TRUE = " 1=1 ";
         const string FALSE = " 1=2 ";

         #region Normalize to: Compare(arg1,arg2) <operand> <compareValue>
         if (right != null)
         {
            // normalize compare method to the left, compareValue to the right
            compareMethod = right;
            compareValue = int.Parse(node.Left.GetExpressionValue().ToString());
            switch (@operator)
            {
               case ExpressionType.LessThan:
                  @operator = ExpressionType.GreaterThan;
                  break;

               case ExpressionType.LessThanOrEqual:
                  @operator = ExpressionType.GreaterThanOrEqual;
                  break;

               case ExpressionType.GreaterThan:
                  @operator = ExpressionType.LessThan;
                  break;

               case ExpressionType.GreaterThanOrEqual:
                  @operator = ExpressionType.LessThanOrEqual;
                  break;
            }
         }
         else
         {
            compareMethod = left;
            compareValue = int.Parse(node.Right.GetExpressionValue().ToString());
         }

         if (compareMethod.Method == KnownMembers.String.Compare)
         {
            arg1 = compareMethod.Object;
            arg2 = compareMethod.Arguments[0];
         }
         else
         {
            arg1 = compareMethod.Arguments[0];
            arg2 = compareMethod.Arguments[1];
         }
         #endregion


         #region Still handle silly arguments....
         if (compareValue > 1)
         {
            switch (@operator)
            {
               case ExpressionType.LessThan:
               case ExpressionType.LessThanOrEqual:
               case ExpressionType.NotEqual:
                  _sb.Append(TRUE);
                  return true;
               default:
                  _sb.Append(FALSE);
                  return true;
            }
         }

         if (compareValue < -1)
         {
            switch (@operator)
            {
               case ExpressionType.GreaterThan:
               case ExpressionType.GreaterThanOrEqual:
               case ExpressionType.NotEqual:
                  _sb.Append(TRUE); // always true
                  return true;
               default:
                  _sb.Append(FALSE); // always false
                  return true;
            }
         }
         #endregion


         if (compareValue == 1)
         {
            switch (@operator)
            {
               case ExpressionType.GreaterThan:
                  _sb.Append(FALSE); // always false 
                  return true;
               case ExpressionType.LessThanOrEqual:
                  _sb.Append(TRUE); // always true
                  return true;
               case ExpressionType.GreaterThanOrEqual:
               case ExpressionType.Equal:
                  Visit(arg1);
                  _sb.Append(" < ");
                  Visit(arg2);
                  return true;
               case ExpressionType.LessThan:
               case ExpressionType.NotEqual:
                  Visit(arg1);
                  _sb.Append(" >= ");
                  Visit(arg2);
                  return true;
               default:
                  throw new ArgumentException();
            }
         }

         if (compareValue == 0)
         {
            switch (@operator)
            {
               case ExpressionType.GreaterThan:
                  Visit(arg1);
                  _sb.Append(" < ");
                  Visit(arg2);
                  return true;
               case ExpressionType.GreaterThanOrEqual:
                  Visit(arg1);
                  _sb.Append(" <= ");
                  Visit(arg2);
                  return true;
               case ExpressionType.Equal:
                  Visit(arg1);
                  _sb.Append(" = ");
                  Visit(arg2);
                  return true;
               case ExpressionType.LessThan:
                  Visit(arg1);
                  _sb.Append(" > ");
                  Visit(arg2);
                  return true;
               case ExpressionType.LessThanOrEqual:
                  Visit(arg1);
                  _sb.Append(" >= ");
                  Visit(arg2);
                  return true;
               case ExpressionType.NotEqual:
                  Visit(arg1);
                  _sb.Append(" <> ");
                  Visit(arg2);
                  return true;
               default:
                  throw new ArgumentException();
            }
         }

         if (compareValue == -1)
         {
            switch (@operator)
            {
               case ExpressionType.LessThan:
                  _sb.Append(FALSE); // always false
                  return true;
               case ExpressionType.GreaterThanOrEqual:
                  _sb.Append(TRUE); // always true
                  return true;
               case ExpressionType.LessThanOrEqual:
               case ExpressionType.Equal:
                  Visit(arg1);
                  _sb.Append(" > ");
                  Visit(arg2);
                  return true;
               case ExpressionType.GreaterThan:
               case ExpressionType.NotEqual:
                  Visit(arg1);
                  _sb.Append(" <= ");
                  Visit(arg2);
                  return true;
               default:
                  throw new ArgumentException();
            }
         }
         return true;

      }

      private static bool IsStringCompare(BinaryExpression node, out MethodCallExpression left, out MethodCallExpression right)
      {
         left = node.Left.TrimConvert() as MethodCallExpression;
         right = node.Right.TrimConvert() as MethodCallExpression;
         var isStringCompare =
            !(left != null && right != null)
            &&
            (
               left != null && KnownMembers.CompareMethods.Contains(left.Method)
               ||
               right != null && KnownMembers.CompareMethods.Contains(right.Method)
               )
            &&
            (
               node.NodeType == ExpressionType.LessThan
               || node.NodeType == ExpressionType.LessThanOrEqual
               || node.NodeType == ExpressionType.GreaterThan
               || node.NodeType == ExpressionType.GreaterThanOrEqual
               || node.NodeType == ExpressionType.Equal
               || node.NodeType == ExpressionType.NotEqual
               );
         return isStringCompare;
      }
   }
}
