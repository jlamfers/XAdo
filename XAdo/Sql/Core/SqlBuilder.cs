using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XAdo.Core;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   public class SqlBuilder : ExpressionVisitor
   {


      public class ParseResult
      {
         public ParseResult(string sql, IDictionary<string, object> arguments)
         {
            Arguments = arguments;
            Sql = sql;
         }

         public string Sql { get; private set; }
         public IDictionary<string, object> Arguments { get; private set; }
      }


      private static readonly Dictionary<ExpressionType, string>
         Operators = new Dictionary<ExpressionType, string>
         {
            {ExpressionType.AndAlso,"AND"},
            {ExpressionType.OrElse,"OR"},
            {ExpressionType.Divide,"/"},
            {ExpressionType.GreaterThan,">"},
            {ExpressionType.GreaterThanOrEqual,">="},
            {ExpressionType.LessThan,"<"},
            {ExpressionType.LessThanOrEqual,"<="},
            {ExpressionType.Multiply,"*"},
            {ExpressionType.Subtract,"-"},
         };


      private readonly ISqlDialect
         _dialect;

      private readonly string _parameterPrefix;

      private readonly bool _noargs;

      private IDictionary<string,object> 
         _arguments;

      private TextWriter
         _writer;

      private IDictionary<string, string> 
         _fullnameToColumnMap;

      private LambdaExpression 
         _expression;

      public SqlBuilder(ISqlDialect dialect, string parameterPrefix="p_", bool noargs = false)
      {
         _dialect = dialect;
         _parameterPrefix = parameterPrefix;
         _noargs = noargs;
         _dialect.EnsureAnnotated();
      }

      public ParseResult Parse(LambdaExpression expression, IDictionary<string, string> fullnameColumnMap, IDictionary<string, object> arguments)
      {
         _fullnameToColumnMap = fullnameColumnMap;
         _arguments = arguments ?? new Dictionary<string, object>();
         _expression = expression;
         using (var writer = new StringWriter())
         {
            _writer = writer;
            Visit(expression);
            _writer = null;
            _expression = null;
            return new ParseResult(writer.GetStringBuilder().ToString(), _arguments);
         }
      }

      protected override Expression VisitUnary(UnaryExpression node)
      {
         switch (node.NodeType)
         {
            case ExpressionType.Not:
               if (node.Operand.Type.EnsureNotNullable() == typeof (bool))
               {
                  FormatSql(" ("+_dialect.BitwiseNot+")", node.Operand);
               }
               else
               {
                  FormatSql(" NOT({0})", node.Operand);
               }
               return node;
            case ExpressionType.Negate:
               FormatSql(" -({0})", node.Operand);
               return node;
         }
         return base.VisitUnary(node);
      }

      protected override Expression VisitConstant(ConstantExpression node)
      {
         if (node.Value == null || node.Value == DBNull.Value)
         {
            _writer.Write(" NULL");
         }
         else if ((node.Value as Type) != null)
         {
            try
            {
               _writer.Write(" ");
               _writer.Write(_dialect.TypeMap[(Type) node.Value]);
            }
            catch (Exception ex)
            {
               throw new Exception("No type map found for type: " + ((Type)node.Value).Name,ex);
            }
         }
         else
         {
            HandleValue(node.Value,null);
         }
         return node;
      }

      protected override Expression VisitBinary(BinaryExpression node)
      {

         if (CheckStringCompare(node.Left, node.Right, node.NodeType))
         {
            return node;
         }

         string opr;

         if (Operators.TryGetValue(node.NodeType, out opr))
         {
            _writer.Write("(");
            Visit(node.Left);
            _writer.Write(" ");
            _writer.Write(opr);
            _writer.Write(" ");
            Visit(node.Right);
            _writer.Write(")");
            return node;
         }

         _writer.Write("(");


         switch (node.NodeType)
         {
            case ExpressionType.Add:
               if (node.Left.Type == typeof (string))
               {
                  FormatSql(_dialect.StringConcat, node.Left, node.Right);
               }
               else
               {
                  Visit(node.Left);
                  _writer.Write(" + ");
                  Visit(node.Right);
               }
               break;
            case ExpressionType.Coalesce:
               FormatSql(_dialect.Coalesce, node.Left, node.Right);
               break;
            case ExpressionType.And:
               FormatSql(_dialect.BitwiseAnd, node.Left, node.Right);
               break;
            case ExpressionType.ExclusiveOr:
               FormatSql(_dialect.BitwiseXOR, node.Left, node.Right);
               break;
            case ExpressionType.Modulo:
               FormatSql(_dialect.Modulo, node.Left, node.Right);
               break;
            case ExpressionType.Equal:
               Visit(node.Left);
               _writer.Write(node.Right.IsNullConstant() ? " IS " : " = "); // stick to SQL-92
               Visit(node.Right);
               break;
            case ExpressionType.NotEqual:
               Visit(node.Left);
               _writer.Write(node.Right.IsNullConstant() ? " IS NOT " : " <> "); // stick to SQL-92
               Visit(node.Right);
               break;
            case ExpressionType.Or:
               FormatSql(_dialect.BitwiseOr, node.Left, node.Right);
               break;
            case ExpressionType.Power:
               FormatSql(_dialect.Power, node.Left, node.Right);
               break;
            default:
               throw new ArgumentOutOfRangeException("binary operator " + node.NodeType+" is not supported");
         }
         _writer.Write(")");
         return node;
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         var formatter = node.Member.GetSqlFormatAttribute(_dialect);
         if (formatter != null)
         {
            FormatSql(formatter.GetFormat(_dialect), node.Expression);
            return node;
         }

         if (node.Member.Name == "Value" && node.Member.DeclaringType.IsNullable())
         {
            // nullable
            return Visit(node.Expression);
         }

         if (node.Member.Name == "HasValue" && node.Member.DeclaringType.IsNullable())
         {
            // defacto standard property name that returns true if the corresponding object represents a not a null value
            Visit(Expression.MakeBinary(ExpressionType.NotEqual, node.Expression, Expression.Constant(null)));
            return node;
         }

         var namePath = new StringBuilder();
         if (node.IsParameterDependent(namePath))
         {
            // throws exception on missing map
            try
            {
               _writer.Write(_fullnameToColumnMap[namePath.ToString()]);
            }
            catch (Exception ex)
            {
               throw new Exception("Member '" + namePath+"' not found. This member could not be mapped to any database column in select expression "+_expression,ex);
            }
         }
         else
         {
            HandleValue(node.GetExpressionValue(),node.Member.Name);
         }
         return node;
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         var formatter = node.Method.GetSqlFormatAttribute(_dialect);
         if (formatter != null)
         {
            FormatSql(formatter.GetFormat(_dialect),node.GetAllArguments().ToArray());
            return node;
         }

         if (node.Method.Name == "GetValueOrDefault" && node.Method.DeclaringType.IsNullable())
         {
            if (node.Arguments.Any())
            {
               FormatSql(_dialect.Coalesce,node.Object, node.Arguments[0]);
               return node;
            }
            return Visit(node.Object);
         }

         return base.VisitMethodCall(node);
      }

      private void FormatSql(string format, params Expression[] args)
      {
         format.FormatSql(_writer, args.Select(a => Parameterize(() => Visit(a))).ToArray());
      }

      private Action<TextWriter> Parameterize(Action action)
      {
         var parw = _writer;
         return w =>
         {
            if (w == null || ReferenceEquals(w, parw))
            {
               action();
            }
            else
            {
               var saved = _writer;
               _writer = w;
               action();
               _writer = saved;
            }
         };
      }

      private void HandleValue(object value, string name)
      {
         if (_noargs)
         {
            _dialect.FormatValue(_writer, value);
         }
         else
         {
            var parameterName = _parameterPrefix + _arguments.Count;
            _writer.Write(_dialect.ParameterFormat, parameterName);
            _arguments[parameterName] = value;
         }
      }

      #region Handle String Compare
      // This region implements support for "normal" string comparison in C#, you may use the type cast to CString as well (using string extension method AsComparable())
      // which does support all compare operators 
      private static readonly Dictionary<ExpressionType, ExpressionType> ReverseCompareOperators = new Dictionary<ExpressionType, ExpressionType>
        {
            {ExpressionType.LessThan,ExpressionType.GreaterThan},
            {ExpressionType.LessThanOrEqual,ExpressionType.GreaterThanOrEqual},
            {ExpressionType.GreaterThan,ExpressionType.LessThan},
            {ExpressionType.GreaterThanOrEqual,ExpressionType.LessThanOrEqual},
            {ExpressionType.Equal,ExpressionType.Equal},
            {ExpressionType.NotEqual,ExpressionType.NotEqual},
        };

      private bool CheckStringCompare(Expression left, Expression right, ExpressionType op)
      {
         if ((left.NodeType != ExpressionType.Call && right.NodeType != ExpressionType.Call) ||
             (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Call) ||
             !ReverseCompareOperators.ContainsKey(op))
         {
            return false;
         }

         if (left.NodeType != ExpressionType.Call)
         {
            op = ReverseCompareOperators[op];
            var tmp = left;
            left = right;
            right = tmp;
         }

         var callExpression = (MethodCallExpression)left;

         try
         {
            if (callExpression.Method == KnownMembers.String.Compare)
            {
               FormatCompare(callExpression.Arguments[0], callExpression.Arguments[1], (int)right.GetExpressionValue(), op);
               return true;
            }
            if (callExpression.Method == KnownMembers.String.CompareTo)
            {
               FormatCompare(callExpression.Object, callExpression.Arguments[0], (int)right.GetExpressionValue(), op);
               return true;
            }
         }
         catch (Exception ex)
         {
            throw new Exception("You must compare to a constant int value, e.g., -1,0,1", ex);
         }
         return false;
      }

      private static readonly Dictionary<Tuple<ExpressionType, int>, string> ResultSqlOperators = new Dictionary<Tuple<ExpressionType, int>, string>
        {
            {Tuple.Create(ExpressionType.LessThanOrEqual,-1), "<"},
            {Tuple.Create(ExpressionType.LessThanOrEqual, 0), "<="},
            {Tuple.Create(ExpressionType.LessThanOrEqual, 1), "true"},
            {Tuple.Create(ExpressionType.GreaterThanOrEqual,-1), "true"},
            {Tuple.Create(ExpressionType.GreaterThanOrEqual, 0), ">="},
            {Tuple.Create(ExpressionType.GreaterThanOrEqual, 1), ">"},
            {Tuple.Create(ExpressionType.LessThan,-1), "false"},
            {Tuple.Create(ExpressionType.LessThan, 0), "<"},
            {Tuple.Create(ExpressionType.LessThan, 1), "<="},
            {Tuple.Create(ExpressionType.GreaterThan,-1), ">="},
            {Tuple.Create(ExpressionType.GreaterThan, 0), ">"},
            {Tuple.Create(ExpressionType.GreaterThan, 1), "false"},
            {Tuple.Create(ExpressionType.Equal,-1), "<"},
            {Tuple.Create(ExpressionType.Equal, 0), "="},
            {Tuple.Create(ExpressionType.Equal, 1), ">"},
            {Tuple.Create(ExpressionType.NotEqual,-1), ">="},
            {Tuple.Create(ExpressionType.NotEqual, 0), "<>"},
            {Tuple.Create(ExpressionType.NotEqual, 1), "<="}
        };

      private void FormatCompare(Expression left, Expression right, int compareResult, ExpressionType op)
      {
         compareResult = Math.Min(Math.Max(compareResult, -1), 1);
         var sqlOp = ResultSqlOperators[Tuple.Create(op, compareResult)];
         if (sqlOp.Length > 2)
         {
            // "true" or "false"
            _writer.Write(sqlOp == "true" ? "(1=1)" : "(1<>1)");
            return;
         }
         _writer.Write("(");
         Visit(left);
         _writer.Write(" ");
         _writer.Write(sqlOp);
         _writer.Write(" ");
         Visit(right);
         _writer.Write(")");
      }

      #endregion


   }
}
