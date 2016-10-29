using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Attributes;
using XAdo.Quobs.Meta;
using XAdo.Quobs.Sql.Formatter;

namespace XAdo.Quobs.Expressions
{
   public class WhereClauseCompiler : ExpressionVisitor
   {
      public class CompileResult
      {
         public CompileResult(string sql, IDictionary<string, object> arguments, Dictionary<string, SqlDescriptor.JoinDescriptor> joins)
         {
            SqlWhereClause = sql;
            Arguments = arguments;
            Joins = joins;
         }

         public string SqlWhereClause { get; private set; }
         public IDictionary<string, object> Arguments { get; private set; }
         public Dictionary<string, SqlDescriptor.JoinDescriptor> Joins { get; private set; }
      }

      private Dictionary<string, SqlDescriptor.JoinDescriptor>
         _joins;
      private Dictionary<string, object>
         _arguments;

      private readonly ISqlFormatter 
         _formatter;

      public static class KnownMembers
      {
         public static class String
         {
            public static readonly MethodInfo
               StartsWith = MemberInfoFinder.GetMethodInfo<string>(s => s.StartsWith("")),
               EndsWith = MemberInfoFinder.GetMethodInfo<string>(s => s.EndsWith("")),
               Contains = MemberInfoFinder.GetMethodInfo<string>(s => s.Contains("")),
               CompareTo = MemberInfoFinder.GetMethodInfo<string>(s => s.CompareTo("")),
               Compare = MemberInfoFinder.GetMethodInfo<bool>(s => string.Compare("", ""));
         }

         public static HashSet<MethodInfo> LikeMethods = new HashSet<MethodInfo>(new[]
         {
            String.Contains,String.EndsWith,String.StartsWith
         });
         public static HashSet<MethodInfo> CompareMethods = new HashSet<MethodInfo>(new[]
         {
            String.CompareTo, String.Compare
         });

      }

      public WhereClauseCompiler(ISqlFormatter formatter)
      {
         if (formatter == null) throw new ArgumentNullException("formatter");
         _formatter = formatter;
      }


      public CompileResult Compile(Expression expression)
      {
         Writer = new StringWriter();
         _arguments = new Dictionary<string, object>();
         _joins = new Dictionary<string, SqlDescriptor.JoinDescriptor>();
         Visit(expression);
         return new CompileResult(Writer.CastTo<StringWriter>().GetStringBuilder().ToString(),_arguments,_joins);
      }

      protected override Expression VisitUnary(UnaryExpression node)
      {
         switch (node.NodeType)
         {
            case ExpressionType.Not:
               Writer.Write(" NOT ");
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
            Writer.Write("(");
         }
         Visit(node.Left);

         switch (node.NodeType)
         {
            case ExpressionType.AndAlso:
               Writer.Write(" AND ");
               break;

            case ExpressionType.OrElse:
               Writer.Write(" OR ");
               break;

            case ExpressionType.Equal:
               Writer.Write(IsNullConstant(node.Right) ? " IS " : " = ");
               break;

            case ExpressionType.NotEqual:
               Writer.Write(IsNullConstant(node.Right) ? " IS NOT " : " <> ");
               break;

            case ExpressionType.LessThan:
               Writer.Write(" < ");
               break;

            case ExpressionType.LessThanOrEqual:
               Writer.Write(" <= ");
               break;

            case ExpressionType.GreaterThan:
               Writer.Write(" > ");
               break;

            case ExpressionType.GreaterThanOrEqual:
               Writer.Write(" >= ");
               break;

            default:
               throw new NotSupportedException(string.Format("Expression not supported: {0}", node));

         }

         Visit(node.Right);
         if (node.NodeType == ExpressionType.OrElse)
         {
            Writer.Write(")");
         }
         return node;
      }
      protected override Expression VisitMember(MemberExpression node)
      {
         bool joined = false;
         if (node.Expression != null && (node.Expression.NodeType == ExpressionType.Parameter || (joined = node.Expression.NodeType == ExpressionType.Call && node.Expression.CastTo<MethodCallExpression>().Method.GetCustomAttribute<JoinMethodAttribute>() != null)))
         {
            _formatter.FormatColumn(Writer,node.Member);
            if (joined)
            {
               return base.VisitMember(node);
            }
            return node;
         }
         if (node.Expression == null)
         {
            // static member, e.g., DateTime.Now
            RegisterArgument(node.GetExpressionValue());
            return node;
         }
         return base.VisitMember(node);
      }
      protected override Expression VisitConstant(ConstantExpression node)
      {
         if (node.Value == null)
         {
            Writer.Write("NULL");
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
            Writer.Write(" LIKE ");
            if (node.Arguments[0].IsParameterMember())
            {
               const string methodName = "CONCAT";
               // we cannot evaluate values if the operand is a parameter member, so we need a column reference
               if (m == KnownMembers.String.StartsWith)
               {
                  FormatSqlMethod(methodName, () => Visit(node.Arguments[0]), () => Writer.Write("'%'"));
               }
               else if (m == KnownMembers.String.EndsWith)
               {
                  FormatSqlMethod(methodName,  () => Writer.Write("'%'"), () => Visit(node.Arguments[0]));
               }
               else if (m == KnownMembers.String.Contains)
               {
                  FormatSqlMethod(methodName, () => Writer.Write("'%'"), () => Visit(node.Arguments[0]), () => Writer.Write("'%'"));
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
         else if (node.IsJoinMethod())
         {
            var joinType = node.Arguments.Count > 1 ? (JoinType) node.Arguments[1].GetExpressionValue() : default(JoinType);
            foreach(var join in node.GetJoinDescriptors())
            {
               // correlation names??
               join.JoinType = joinType;
               if (!_joins.ContainsKey(join.Expression))
               {
                  _joins[join.Expression] = new SqlDescriptor.JoinDescriptor(join);
               }
            }
            Visit(node.Arguments[0]);
         }
         else
         {
            RegisterArgument(node.GetExpressionValue());
         }

         return node;
      }

      protected TextWriter Writer { get; private set; }

      protected void FormatSqlMethod(string methodName, params Action[] args)
      {
         _formatter.FormatSqlMethod(methodName, Writer, args.Select(ParameterizeWriter).ToArray());
      }

      private Action<TextWriter> ParameterizeWriter(Action action)
      {
         return w =>
         {
            var pw = Writer;
            Writer = w;
            action();
            Writer = pw;
         };
      }

      private void RegisterArgument(object value)
      {
         var argumentName = "arg_" + _arguments.Count;
         _formatter.FormatParameterName(Writer,argumentName);
         _arguments[argumentName] = value;
      }

      private bool IsNullConstant(Expression exp)
      {
         return exp.NodeType==ExpressionType.Convert ? IsNullConstant(exp.CastTo<UnaryExpression>().Operand) : (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
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

         if (compareMethod.Method == KnownMembers.String.CompareTo)
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
                  Writer.Write(TRUE);
                  return true;
               default:
                  Writer.Write(FALSE);
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
                  Writer.Write(TRUE); // always true
                  return true;
               default:
                  Writer.Write(FALSE); // always false
                  return true;
            }
         }
         #endregion


         if (compareValue == 1)
         {
            switch (@operator)
            {
               case ExpressionType.GreaterThan:
                  Writer.Write(FALSE); // always false 
                  return true;
               case ExpressionType.LessThanOrEqual:
                  Writer.Write(TRUE); // always true
                  return true;
               case ExpressionType.GreaterThanOrEqual:
               case ExpressionType.Equal:
                  Visit(arg1);
                  Writer.Write(" < ");
                  Visit(arg2);
                  return true;
               case ExpressionType.LessThan:
               case ExpressionType.NotEqual:
                  Visit(arg1);
                  Writer.Write(" >= ");
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
                  Writer.Write(" < ");
                  Visit(arg2);
                  return true;
               case ExpressionType.GreaterThanOrEqual:
                  Visit(arg1);
                  Writer.Write(" <= ");
                  Visit(arg2);
                  return true;
               case ExpressionType.Equal:
                  Visit(arg1);
                  Writer.Write(" = ");
                  Visit(arg2);
                  return true;
               case ExpressionType.LessThan:
                  Visit(arg1);
                  Writer.Write(" > ");
                  Visit(arg2);
                  return true;
               case ExpressionType.LessThanOrEqual:
                  Visit(arg1);
                  Writer.Write(" >= ");
                  Visit(arg2);
                  return true;
               case ExpressionType.NotEqual:
                  Visit(arg1);
                  Writer.Write(" <> ");
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
                  Writer.Write(FALSE); // always false
                  return true;
               case ExpressionType.GreaterThanOrEqual:
                  Writer.Write(TRUE); // always true
                  return true;
               case ExpressionType.LessThanOrEqual:
               case ExpressionType.Equal:
                  Visit(arg1);
                  Writer.Write(" > ");
                  Visit(arg2);
                  return true;
               case ExpressionType.GreaterThan:
               case ExpressionType.NotEqual:
                  Visit(arg1);
                  Writer.Write(" <= ");
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
