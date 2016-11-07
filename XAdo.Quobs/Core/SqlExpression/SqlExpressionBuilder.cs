using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Core.SqlExpression.Sql;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace XAdo.Quobs.Core.SqlExpression
{
   public class SqlExpressionBuilder : ExpressionVisitor
   {
      #region Writer Method Mappings

      private static readonly Dictionary<MethodInfo, Func<SqlExpressionBuilder, MethodCallExpression, Expression>>
          MethodWriterMap = new Dictionary<MethodInfo, Func<SqlExpressionBuilder, MethodCallExpression, Expression>>
            {
                {KnownMembers.String.StartsWith, WriteStartsWith},
                {KnownMembers.String.EndsWith, WriteEndsWith},
                {KnownMembers.String.Contains, WriteContains},
                {KnownMembers.String.Equals, (w, e) => w.ParseCompareExpression(e, "=")},
                {KnownMembers.String.EqualsStatic, (w, e) => w.ParseCompareExpression(e, "=")},
                {KnownMembers.String.ToUpper, WriteToUpper},
                {KnownMembers.String.ToLower, WriteToLower},
                {KnownMembers.DateTime.AddMilliseconds, WriteAddMilliseconds},
                {KnownMembers.DateTime.AddSeconds, WriteAddSeconds},
                {KnownMembers.DateTime.AddMinutes, WriteAddMinutes},
                {KnownMembers.DateTime.AddHours, WriteAddHours},
                {KnownMembers.DateTime.AddDays, WriteAddDays},
                {KnownMembers.DateTime.AddMonths, WriteAddMonths},
                {KnownMembers.DateTime.AddYears, WriteAddYears},
                {KnownMembers.Math.FloorDouble, WriteFloor},
                {KnownMembers.Math.FloorDecimal, WriteFloor},
                {KnownMembers.Math.CeilingDouble, WriteCeiling},
                {KnownMembers.Math.CeilingDecimal, WriteCeiling},
                {KnownMembers.Math.RoundDouble, WriteRound},
                {KnownMembers.Math.RoundDecimal, WriteRound},
                {KnownMembers.Math.RoundDoubleZeroDigits, WriteRoundZeroDigits},
                {KnownMembers.Math.RoundDecimalZeroDigits, WriteRoundZeroDigits},
            };


      private static readonly IDictionary<ExpressionType, Func<SqlExpressionBuilder, BinaryExpression, Expression>>
          OperatorWriterMap = new Dictionary<ExpressionType, Func<SqlExpressionBuilder, BinaryExpression, Expression>>
            {
                {ExpressionType.AndAlso, (w,e) => w.FormatBinary(e," AND ")},
                {ExpressionType.OrElse, (w,e) => w.FormatBinary(e," OR ")},
                {ExpressionType.Equal, (w,e) => w.FormatBinary(e,IsNullConstant(e.Right) ? " IS " : " = ")},
                {ExpressionType.NotEqual, (w,e) => w.FormatBinary(e,IsNullConstant(e.Right) ? " IS NOT " : " <> ")},
                {ExpressionType.LessThan, (w,e) => w.FormatBinary(e," < ")},
                {ExpressionType.LessThanOrEqual, (w,e) => w.FormatBinary(e," <= ")},
                {ExpressionType.GreaterThan, (w,e) => w.FormatBinary(e," > ")},
                {ExpressionType.GreaterThanOrEqual, (w,e) => w.FormatBinary(e," >= ")},
                {ExpressionType.Subtract, (w,e) => w.FormatBinary(e," - ")},
                {ExpressionType.Add, (w,e) => w.FormatBinary(e," + ")},
                {ExpressionType.Divide, (w,e) => w.FormatBinary(e," / ")},
                {ExpressionType.Multiply, (w,e) => w.FormatBinary(e," * ")},
                {ExpressionType.Coalesce, WriteCoalesce},
                {ExpressionType.Modulo, WriteModulo},
            };


      private static readonly Dictionary<PropertyInfo, Func<SqlExpressionBuilder, MemberExpression, Expression>>
          PropertyWriterMap = new Dictionary<PropertyInfo, Func<SqlExpressionBuilder, MemberExpression, Expression>>
            {
                {KnownMembers.DateTime.Now, WriteDateTimeNow},
                {KnownMembers.DateTime.UtcNow, WriteDateTimeUtcNow},
                {KnownMembers.DateTime.Today, WriteDateTimeToday},
                {KnownMembers.DateTime.Date, WriteDateTimeDate},
                {KnownMembers.DateTime.Year, WriteDateTimeYear},
                {KnownMembers.DateTime.Month, WriteDateTimeMonth},
                {KnownMembers.DateTime.Day, WriteDateTimeDay},
                {KnownMembers.DateTime.DayOfWeek, WriteDateTimeDayOfWeek},
                {KnownMembers.DateTime.DayOfYear, WriteDateTimeDayOfYear},
                {KnownMembers.DateTime.Hour, WriteDateTimeHour},
                {KnownMembers.DateTime.Minute, WriteDateTimeMinute},
                {KnownMembers.DateTime.Second, WriteDateTimeSecond},
                {KnownMembers.DateTime.Millisecond, WriteDateTimeMillisecond},
                {KnownMembers.String.Length, WriteStringLength},
            };

      
      #endregion


      private SqlBuilderContext _context;
      private TextWriter _writer;
      private ISqlFormatter _formatter;


      public SqlBuilderContext BuildSql(SqlBuilderContext context, Expression expression)
      {
         if (context == null) throw new ArgumentNullException("context");
         if (expression == null) return context;
         _context = context;
         _writer = _context.Writer;
         _formatter = _context.Formatter;
         Visit(expression);
         return context;

      }

      #region ExpressionVisitor Overrides

      protected override Expression VisitMethodCall(MethodCallExpression exp)
      {
         Expression visitResult;
         if (HookVisit(exp, out visitResult))
         {
            return visitResult;
         }

         var customBuilder = exp.Method.GetAnnotation<CustomSqlExpressionBuilderAttribute>();
         if (customBuilder != null)
         {
            customBuilder.Builder.BuildSql(this,_context, exp);
            return exp;
         }

         if (exp.Method.Name == "GetValueOrDefault" && Nullable.GetUnderlyingType(exp.Method.DeclaringType) != null)
         {
            if (exp.Arguments.Count > 0)
            {
               var self = this;
               _formatter.WriteCoalesce(_writer, ParameterizeWriter(() => Visit(exp.Object), self), ParameterizeWriter(() => Visit(exp.Arguments[0]), self));
               return exp;
            }
            return Visit(exp.Object);
         }

         Func<SqlExpressionBuilder, MethodCallExpression, Expression> Writer;
         if (MethodWriterMap.TryGetValue(exp.Method, out Writer))
         {
            return Writer(this, exp);
         }

         throw new NotSupportedException(string.Format("The method '{0}' is not supported", exp.Method.Name));
      }

      protected override Expression VisitUnary(UnaryExpression exp)
      {
         Expression visitResult;
         if (HookVisit(exp, out visitResult))
         {
            return visitResult;
         }

         switch (exp.NodeType)
         {
            case ExpressionType.Not:
               Write(" NOT(");
               Visit(exp.Operand);
               Write(")");
               break;

            case ExpressionType.Quote:
               Visit(exp.Operand);
               break;

            case ExpressionType.Convert:
               _formatter.WriteTypeCast(_writer, exp.Type, ParameterizeWriter(() => Visit(exp.Operand),this));
               break;

            default:
               throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", exp.NodeType));
         }
         return exp;
      }

      protected override Expression VisitBinary(BinaryExpression exp)
      {
         if (exp.NodeType == ExpressionType.Add)
         {
            if (exp.Left.Type == typeof (string) && exp.Right.Type == typeof (string))
            {
               _formatter.WriteConcatenate(_writer, ParameterizeWriter(() => Visit(exp.Left), this), ParameterizeWriter(() => Visit(exp.Right), this));
               return exp;
            }
         }
         if (CheckStringCompare(exp.Left, exp.Right, exp.NodeType))
         {
            return exp;
         }

         Expression visitResult;
         if (HookVisit(exp, out visitResult))
         {
            return visitResult;
         }

         Func<SqlExpressionBuilder, BinaryExpression, Expression> wrtr;
         if (!OperatorWriterMap.TryGetValue(exp.NodeType, out wrtr))
         {
            throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", exp.NodeType));
         }

         return wrtr(this, exp);
      }

      protected override Expression VisitConstant(ConstantExpression exp)
      {
         Expression visitResult;
         if (HookVisit(exp, out visitResult))
         {
            return visitResult;
         }
         Write(_formatter.FormatValue(exp.Value));

         return exp;
      }

      protected override Expression VisitMember(MemberExpression exp)
      {

         Expression visitResult;
         if (HookVisit(exp, out visitResult))
         {
            return visitResult;
         }

         var customBuilder = exp.Member.GetAnnotation<CustomSqlExpressionBuilderAttribute>();
         if (customBuilder != null)
         {
            customBuilder.Builder.BuildSql(this, _context, exp);
            return exp;
         }

         if (exp.Member.Name == "Value" && (Nullable.GetUnderlyingType(exp.Member.DeclaringType) != null || exp.Member.DeclaringType.Name.StartsWith("Sql")))
         {
            // nullable or SqlType
            return Visit(exp.Expression);
         }

         if (exp.Member.Name == "HasValue"&& Nullable.GetUnderlyingType(exp.Member.DeclaringType) != null )
         {
            // defacto standard property name that returns true if the corresponding object represents a not a null value
            Visit(Expression.MakeBinary(ExpressionType.NotEqual, exp.Expression, Expression.Constant(null)));
            return exp;
         }

         Func<SqlExpressionBuilder, MemberExpression, Expression> generator;
         if (exp.Member.MemberType == MemberTypes.Property && PropertyWriterMap.TryGetValue((PropertyInfo)exp.Member, out generator))
         {
            return generator(this, exp);

         }

         if (exp.Expression == null)
         {
            HandleArgumentMember(exp,exp.GetExpressionValue());
            return exp;
         }

         switch (exp.Expression.NodeType)
         {
            case ExpressionType.Parameter:
               WriteFormattedColumn(exp.Member);
               break;
            default:
               object result;
               if (exp.TryEvaluate(out result))
               {
                  HandleArgumentMember(exp, result);
               }
               else
               {
                  WriteFormattedColumn(exp.Member);
                  var node = Visit(exp.Expression);
                  if (node == null)
                  {
                     throw new NotSupportedException(string.Format("The member '{0}' in expression '{1}' is not supported ", exp.Member.Name, exp));
                  }
               }
               break;
         }
         return exp;
      }

      #endregion

      #region Helper methods
      protected virtual void WriteArgument(MemberExpression exp, object value)
      {
         var name = GetParameterName(exp);
         var type = exp.Member.GetMemberType();
         if (value != null)
         {
            if (value.GetType() != type)
            {
               value = Convert.ChangeType(value, type);
            }
         }
         if (_context.ArgumentsAsLiterals)
         {
            Write(_formatter.FormatValue(_formatter.NormalizeValue(value)));
         }
         else
         {
            Write(_formatter.FormatParameter(name));
            SetArgument(name, value);
         }
      }

      protected virtual bool HookVisit(Expression exp, out Expression visitResult)
      {
         if (_context.VisitorHook != null)
         {
            visitResult = _context.VisitorHook(this, _context, exp);
            if (visitResult != null) return true;
         }
         visitResult = _formatter.VisitorHook(this, _context, exp);
         return visitResult != null;
      }

      protected virtual string GetParameterName(MemberExpression exp)
      {
         if (exp == null)
         {
            return null;
         }
         if (exp.Expression == null)
         {
            return string.Format("{0}_{1}", exp.Member.ReflectedType.Name, exp.Member.Name);
         }
         return exp.Member.Name;
      }

      #region Handle String Compare
      // This region implements support for "normal" string comparison in C#, you may use the type cast to CString as well (or using string extension method ToCString())
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
            throw new ApplicationException("You must compare to a constant int value, e.g., -1,0,1", ex);
         }
         return false;
      }

      private class CompareTuple
      {
         private readonly ExpressionType _compareOperator;
         private readonly int _compareResult;
         private readonly int _hashcode;

         public CompareTuple(ExpressionType compareOperator, int compareResult)
         {
            _compareOperator = compareOperator;
            _compareResult = compareResult;
            _hashcode = compareResult * 23 + (int)compareOperator;
         }

         public override int GetHashCode()
         {
            return _hashcode;
         }

         public override bool Equals(object obj)
         {
            var other = (CompareTuple)obj;
            return other._compareOperator == _compareOperator && other._compareResult == _compareResult;
         }
      }

      private static readonly Dictionary<CompareTuple, string> ResultSqlOperators = new Dictionary<CompareTuple, string>
        {
            {new CompareTuple(ExpressionType.LessThanOrEqual,-1), "<"},
            {new CompareTuple(ExpressionType.LessThanOrEqual, 0), "<="},
            {new CompareTuple(ExpressionType.LessThanOrEqual, 1), "true"},
            {new CompareTuple(ExpressionType.GreaterThanOrEqual,-1), "true"},
            {new CompareTuple(ExpressionType.GreaterThanOrEqual, 0), ">="},
            {new CompareTuple(ExpressionType.GreaterThanOrEqual, 1), ">"},
            {new CompareTuple(ExpressionType.LessThan,-1), "false"},
            {new CompareTuple(ExpressionType.LessThan, 0), "<"},
            {new CompareTuple(ExpressionType.LessThan, 1), "<="},
            {new CompareTuple(ExpressionType.GreaterThan,-1), ">="},
            {new CompareTuple(ExpressionType.GreaterThan, 0), ">"},
            {new CompareTuple(ExpressionType.GreaterThan, 1), "false"},
            {new CompareTuple(ExpressionType.Equal,-1), "<"},
            {new CompareTuple(ExpressionType.Equal, 0), "="},
            {new CompareTuple(ExpressionType.Equal, 1), ">"},
            {new CompareTuple(ExpressionType.NotEqual,-1), ">="},
            {new CompareTuple(ExpressionType.NotEqual, 0), "<>"},
            {new CompareTuple(ExpressionType.NotEqual, 1), "<="}
        };

      private void FormatCompare(Expression left, Expression right, int compareResult, ExpressionType op)
      {
         compareResult = Math.Min(Math.Max(compareResult, -1), 1);
         var sqlOp = ResultSqlOperators[new CompareTuple(op, compareResult)];
         if (sqlOp.Length > 2)
         {
            if (sqlOp == "true")
            {
               _formatter.WriteTrueExpression(_writer);
            }
            else
            {
               _formatter.WriteFalseExpression(_writer);
            }
            return;
         }
         Visit(left);
         Write(" ");
         Write(sqlOp);
         Write(" ");
         Visit(right);
      }

      #endregion

      private Expression FormatBinary(BinaryExpression e, string op)
      {
         Write("(");
         Visit(e.Left);
         Write(op);
         Visit(e.Right);
         Write(")");
         return e;
      }

      private void HandleArgumentMember(MemberExpression exp, object value)
      {
         WriteArgument(exp, value);
      }

      private void Write(string s)
      {
         _writer.Write(s);
      }

      private void WritePercentage()
      {
         Write(_formatter.FormatValue("%"));
      }

      private static bool IsNullConstant(Expression exp)
      {
         object value;
         return exp.TryEvaluate(out value) && (value == null || value == DBNull.Value);
      }

      private Expression ParseCompareExpression(MethodCallExpression exp, string op)
      {
         if (exp.Object == null)
         {
            Visit(exp.Arguments[0]);
            Write(op);
            return Visit(exp.Arguments[1]);
         }
         Visit(exp.Object);
         Write(op);
         return Visit(exp.Arguments[0]);
      }

      #endregion

      #region Write methods

      private static Expression WriteContains(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w.Visit(e.Object);
         w.Write(" LIKE ");
         if (e.Arguments[0].NodeType == ExpressionType.Constant)
         {
            w.Write(w._formatter.FormatValue("%" + e.Arguments[0].GetExpressionValue() + "%"));
            return e;
         }
         w._formatter.WriteConcatenate(w._writer, ParameterizeWriter(w.WritePercentage, w),ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(w.WritePercentage, w));
         return e;
      }

      private static Expression WriteStartsWith(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w.Visit(e.Object);
         w.Write(" LIKE ");
         if (e.Arguments[0].NodeType == ExpressionType.Constant)
         {
            w.Write(w._formatter.FormatValue(e.Arguments[0].GetExpressionValue() + "%"));
            return e;
         }
         w._formatter.WriteConcatenate(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(w.WritePercentage, w));
         return e;
      }

      private static Expression WriteEndsWith(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w.Visit(e.Object);
         w.Write(" LIKE ");
         if (e.Arguments[0].NodeType == ExpressionType.Constant)
         {
            w.Write("%" + w._formatter.FormatValue(e.Arguments[0].GetExpressionValue()));
            return e;
         }
         w._formatter.WriteConcatenate(w._writer, ParameterizeWriter(w.WritePercentage, w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteModulo(SqlExpressionBuilder w, BinaryExpression e)
      {
         w._formatter.WriteModulo(w._writer, ParameterizeWriter(() => w.Visit(e.Left), w), ParameterizeWriter(() => w.Visit(e.Right), w));
         return e;
      }

      private static Expression WriteAddYears(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddYears(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddMonths(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddMonths(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddDays(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddDays(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddHours(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddHours(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddMinutes(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddMinutes(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddSeconds(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddSeconds(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddMilliseconds(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddMilliseconds(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteDateTimeMillisecond(SqlExpressionBuilder w, MemberExpression e)
      {
         w._formatter.WriteDateTimeMillisecond(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeSecond(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeSecond(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeMinute(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeMinute(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeHour(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeHour(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDayOfYear(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDayOfYear(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDayOfWeek(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDayOfWeek(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDay(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDay(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeMonth(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeMonth(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeYear(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeYear(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDate(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDate(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeToday(SqlExpressionBuilder w, MemberExpression e)
      {
         w.Write(w._formatter.Today);
         return e;
      }

      private static Expression WriteDateTimeUtcNow(SqlExpressionBuilder w, MemberExpression e)
      {
         w.Write(w._formatter.UtcNow);
         return e;
      }

      private static Expression WriteDateTimeNow(SqlExpressionBuilder w, MemberExpression e)
      {
         w.Write(w._formatter.Now);
         return e;
      }

      private static Expression WriteCoalesce(SqlExpressionBuilder w, BinaryExpression e)
      {
         w._formatter.WriteCoalesce(w._writer, ParameterizeWriter(() => w.Visit(e.Left), w), ParameterizeWriter(() => w.Visit(e.Right), w));
         return e;                              
      }

      private static Expression WriteStringLength(SqlExpressionBuilder w, MemberExpression e)
      {

         w._formatter.WriteStringLength(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteRound(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteRound(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(() => w.Visit(e.Arguments[1]), w));
         return e;
      }

      private static Expression WriteRoundZeroDigits(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteRound(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(() => w._writer.Write("0"), w));
         return e;
      }

      private static Expression WriteCeiling(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteCeiling(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteFloor(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteFloor(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteToUpper(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteToUpper(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w));
         return e;
      }

      private static Expression WriteToLower(SqlExpressionBuilder w, MethodCallExpression e)
      {
         w._formatter.WriteToLower(w._writer, ParameterizeWriter(() => w.Visit(e.Object),w));
         return e;
      }

      #endregion

      private void SetArgument(string name, object arg)
      {
         _context.Arguments[name] = _formatter.NormalizeValue(arg);
      }

      private void WriteFormattedColumn(MemberInfo member)
      {
         _writer.Write(_formatter.MemberFormatter.FormatColumn(_formatter, member));
      }

      private static Action<TextWriter> ParameterizeWriter(Action action, SqlExpressionBuilder sqlWriter)
      {
         var parw = sqlWriter._writer;
         return w =>
         {
            if (w == null || ReferenceEquals(w, parw))
            {
               action();
            }
            else
            {
               var saved = sqlWriter._writer;
               sqlWriter._writer = w;
               action();
               sqlWriter._writer = saved;
            }
         };
      }

   }

}
