using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlExpression.Attributes;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace XAdo.SqlObjects.SqlExpression.Visitors
{
   public class SqlExpressionVisitor : ExpressionVisitor
   {
      #region Writer Method Mappings

      private static readonly Dictionary<MethodInfo, Func<SqlExpressionVisitor, MethodCallExpression, Expression>>
          MethodWriterMap = new Dictionary<MethodInfo, Func<SqlExpressionVisitor, MethodCallExpression, Expression>>
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


      private static readonly IDictionary<ExpressionType, Func<SqlExpressionVisitor, BinaryExpression, Expression>>
          OperatorWriterMap = new Dictionary<ExpressionType, Func<SqlExpressionVisitor, BinaryExpression, Expression>>
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


      private static readonly Dictionary<PropertyInfo, Func<SqlExpressionVisitor, MemberExpression, Expression>>
          PropertyWriterMap = new Dictionary<PropertyInfo, Func<SqlExpressionVisitor, MemberExpression, Expression>>
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


      private TextWriter _writer;
      private ISqlFormatter _formatter;

      protected SqlBuilderContext Context { get; private set; }
      public SqlBuilderContext BuildSql(SqlBuilderContext context, Expression expression)
      {
         if (context == null) throw new ArgumentNullException("context");
         if (expression == null) return context;
         var lambda = expression as LambdaExpression;
         if (lambda != null && lambda.Body.NodeType == ExpressionType.Constant && lambda.Body.Type == typeof (Boolean))
         {
            expression = NormalizeSqlConstantBooleanCompare((ConstantExpression)lambda.Body);
         }
         Context = context;
         _writer = Context.Writer;
         _formatter = Context.Formatter;
         Visit(expression);
         return context;

      }

      private Expression NormalizeSqlConstantBooleanCompare(ConstantExpression expression)
      {
         return Expression.Equal(Expression.Constant(1), Expression.Constant((bool)expression.Value ? 1 : 0));
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
            customBuilder.Builder.BuildSql(this,Context, exp);
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

         Func<SqlExpressionVisitor, MethodCallExpression, Expression> Writer;
         if (MethodWriterMap.TryGetValue(exp.Method, out Writer))
         {
            return Writer(this, exp);
         }

         throw new SqlObjectsException(string.Format("The method '{0}' is not supported", exp.Method.Name));
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
               if (IsNullConstant(exp.Operand))
               {
                  _writer.Write("NULL");
                  break;
               }
               _formatter.WriteTypeCast(_writer, exp.Type, ParameterizeWriter(() => Visit(exp.Operand),this));
               break;

            default:
               throw new SqlObjectsException(string.Format("The unary operator '{0}' is not supported", exp.NodeType));
         }
         return exp;
      }

      protected override Expression VisitBinary(BinaryExpression exp)
      {
         if (exp.NodeType == ExpressionType.AndAlso || exp.NodeType == ExpressionType.OrElse)
         {
            if (exp.Left.NodeType == ExpressionType.Constant || exp.Right.NodeType == ExpressionType.Constant)
            {
               var left = exp.Left.NodeType == ExpressionType.Constant
                  ? NormalizeSqlConstantBooleanCompare(exp.Left.CastTo<ConstantExpression>())
                  : exp.Left;
               var right = exp.Right.NodeType == ExpressionType.Constant
                  ? NormalizeSqlConstantBooleanCompare(exp.Right.CastTo<ConstantExpression>())
                  : exp.Right;
               return VisitBinary(Expression.MakeBinary(exp.NodeType, left, right));
            }
         }

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

         Func<SqlExpressionVisitor, BinaryExpression, Expression> wrtr;
         if (!OperatorWriterMap.TryGetValue(exp.NodeType, out wrtr))
         {
            throw new SqlObjectsException(string.Format("The binary operator '{0}' is not supported", exp.NodeType));
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
         _formatter.FormatValue(_writer, exp.Value);

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
            customBuilder.Builder.BuildSql(this, Context, exp);
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

         Func<SqlExpressionVisitor, MemberExpression, Expression> generator;
         if (exp.Member.MemberType == MemberTypes.Property && PropertyWriterMap.TryGetValue((PropertyInfo)exp.Member, out generator))
         {
            return generator(this, exp);

         }

         if (exp.Expression == null)
         {
            HandleArgumentMember(exp,exp.GetExpressionValue());
            return exp;
         }

         //if (exp.Expression.NodeType == ExpressionType.Convert)
         //{
         //   exp = Expression.MakeMemberAccess(((UnaryExpression) exp.Expression).Operand, exp.Member);
         //}

         switch (exp.Expression.NodeType)
         {
            case ExpressionType.Convert:
            case ExpressionType.Parameter:
               WriteFormattedColumn(exp);
               break;
            default:
               object result;
               if (exp.TryEvaluate(out result))
               {
                  HandleArgumentMember(exp, result);
               }
               else
               {
                  // it must be an indirectly parameter referenced member
                  WriteFormattedColumn(exp);
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
         if (Context.ArgumentsAsLiterals)
         {
            _formatter.FormatValue(_writer,value);
         }
         else
         {
            _formatter.FormatParameter(_writer, name);
            SetArgument(name, value);
         }
      }

      protected virtual bool HookVisit(Expression exp, out Expression visitResult)
      {
         if (Context.VisitorHook != null)
         {
            visitResult = Context.VisitorHook(this, Context, exp);
            if (visitResult != null) return true;
         }
         visitResult = _formatter.VisitorHook(this, Context, exp);
         return visitResult != null;
      }

      protected virtual string GetParameterName(MemberExpression exp)
      {
         var index = Context.LatestArgumentsIndex++;
         return exp == null ? Aliases.Parameter(index) : exp.Member.Name + "_" + index;
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
            throw new SqlObjectsException("You must compare to a constant int value, e.g., -1,0,1", ex);
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
               _formatter.WriteTrue(_writer);
            }
            else
            {
               _formatter.WriteFalse(_writer);
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
         _formatter.FormatValue(_writer, "%");
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

      private static Expression WriteContains(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w.Visit(e.Object);
         w.Write(" LIKE ");
         if (e.Arguments[0].NodeType == ExpressionType.Constant)
         {
            w._formatter.FormatValue(w._writer, "%" + e.Arguments[0].GetExpressionValue() + "%");
            return e;
         }
         w._formatter.WriteConcatenate(w._writer, ParameterizeWriter(w.WritePercentage, w),ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(w.WritePercentage, w));
         return e;
      }

      private static Expression WriteStartsWith(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w.Visit(e.Object);
         w.Write(" LIKE ");
         if (e.Arguments[0].NodeType == ExpressionType.Constant)
         {
            w._formatter.FormatValue(w._writer,e.Arguments[0].GetExpressionValue() + "%");
            return e;
         }
         w._formatter.WriteConcatenate(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(w.WritePercentage, w));
         return e;
      }

      private static Expression WriteEndsWith(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w.Visit(e.Object);
         w.Write(" LIKE ");
         if (e.Arguments[0].NodeType == ExpressionType.Constant)
         {
            w.Write("%");
            w._formatter.FormatValue(w._writer,e.Arguments[0].GetExpressionValue());
            return e;
         }
         w._formatter.WriteConcatenate(w._writer, ParameterizeWriter(w.WritePercentage, w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteModulo(SqlExpressionVisitor w, BinaryExpression e)
      {
         w._formatter.WriteModulo(w._writer, ParameterizeWriter(() => w.Visit(e.Left), w), ParameterizeWriter(() => w.Visit(e.Right), w));
         return e;
      }

      private static Expression WriteAddYears(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddYears(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddMonths(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddMonths(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddDays(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddDays(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddHours(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddHours(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddMinutes(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddMinutes(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddSeconds(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddSeconds(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteAddMilliseconds(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteDateTimeAddMilliseconds(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w), ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteDateTimeMillisecond(SqlExpressionVisitor w, MemberExpression e)
      {
         w._formatter.WriteDateTimeMillisecond(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeSecond(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeSecond(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeMinute(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeMinute(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeHour(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeHour(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDayOfYear(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDayOfYear(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDayOfWeek(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDayOfWeek(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDay(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDay(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeMonth(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeMonth(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeYear(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeYear(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeDate(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteDateTimeDate(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteDateTimeToday(SqlExpressionVisitor w, MemberExpression e)
      {
         w.Write(w._formatter.SqlDialect.Today);
         return e;
      }

      private static Expression WriteDateTimeUtcNow(SqlExpressionVisitor w, MemberExpression e)
      {
         w.Write(w._formatter.SqlDialect.UtcNow);
         return e;
      }

      private static Expression WriteDateTimeNow(SqlExpressionVisitor w, MemberExpression e)
      {
         w.Write(w._formatter.SqlDialect.Now);
         return e;
      }

      private static Expression WriteCoalesce(SqlExpressionVisitor w, BinaryExpression e)
      {
         w._formatter.WriteCoalesce(w._writer, ParameterizeWriter(() => w.Visit(e.Left), w), ParameterizeWriter(() => w.Visit(e.Right), w));
         return e;                              
      }

      private static Expression WriteStringLength(SqlExpressionVisitor w, MemberExpression e)
      {

         w._formatter.WriteStringLength(w._writer, ParameterizeWriter(() => w.Visit(e.Expression), w));
         return e;
      }

      private static Expression WriteRound(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteRound(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(() => w.Visit(e.Arguments[1]), w));
         return e;
      }

      private static Expression WriteRoundZeroDigits(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteRound(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w), ParameterizeWriter(() => w._writer.Write("0"), w));
         return e;
      }

      private static Expression WriteCeiling(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteCeiling(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteFloor(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteFloor(w._writer, ParameterizeWriter(() => w.Visit(e.Arguments[0]), w));
         return e;
      }

      private static Expression WriteToUpper(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteToUpper(w._writer, ParameterizeWriter(() => w.Visit(e.Object), w));
         return e;
      }

      private static Expression WriteToLower(SqlExpressionVisitor w, MethodCallExpression e)
      {
         w._formatter.WriteToLower(w._writer, ParameterizeWriter(() => w.Visit(e.Object),w));
         return e;
      }

      #endregion

      private void SetArgument(string name, object arg)
      {
         Context.Arguments[name] = _formatter.NormalizeValue(arg);
      }

      private void WriteFormattedColumn(MemberExpression exp)
      {
         Context.WriteFormattedColumn(exp);
      }

      private static Action<TextWriter> ParameterizeWriter(Action action, SqlExpressionVisitor sqlWriter)
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
