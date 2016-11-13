using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core.SqlExpression.Sql
{
   public interface ISqlFormatter
   {

      Expression VisitorHook(ExpressionVisitor visitor, SqlBuilderContext context, Expression source);

      object NormalizeValue(object value);
      string DateTimeFormat { get; }
      string IdentifierSeperator { get; }
      string StatementSeperator { get; }
      string IdentifierDelimiterLeft { get; }
      string IdentifierDelimiterRight { get; }
      string ParameterPrefix { get; }
      string Now { get; }
      string Today { get; }
      string UtcNow { get; }
      string FormatColumn(string schema, string table, string column, string alias);
      string FormatTable(string schema, string table, string alias);
      string FormatParameter(string parameterName);
      string FormatValue(object value);
      string FormatType(Type type);

      void WriteConcatenate(TextWriter writer, params Action<TextWriter>[] args);
      void WriteCoalesce(TextWriter writer, params Action<TextWriter>[] args);
      void WriteExists(TextWriter writer, Action<TextWriter> sqlSelect);

      void WriteTrueExpression(TextWriter writer);
      void WriteFalseExpression(TextWriter writer);

      void WriteTypeCast(TextWriter writer, Type type, Action<TextWriter> value);

      void WriteDateTimeWeekNumber(TextWriter writer, Action<TextWriter> date);

      void WriteDateTimeDate(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeYear(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeMonth(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeDay(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeDayOfWeek(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeDayOfYear(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeHour(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeMinute(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeSecond(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeMillisecond(TextWriter writer, Action<TextWriter> date);
      void WriteDateTimeAddDays(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      void WriteDateTimeAddYears(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      void WriteDateTimeAddMonths(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      void WriteDateTimeAddHours(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      void WriteDateTimeAddMinutes(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      void WriteDateTimeAddSeconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      void WriteDateTimeAddMilliseconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);

      void WriteModulo(TextWriter writer, Action<TextWriter> left, Action<TextWriter> right);

      void WriteStringLength(TextWriter writer, Action<TextWriter> arg);
      void WriteToUpper(TextWriter writer, Action<TextWriter> arg);
      void WriteToLower(TextWriter writer, Action<TextWriter> arg);

      void WriteFloor(TextWriter writer, Action<TextWriter> arg);
      void WriteRound(TextWriter writer, Action<TextWriter> arg, Action<TextWriter> length);
      void WriteCeiling(TextWriter writer, Action<TextWriter> arg);

      void WritePagedQuery(TextWriter writer, string sqlSelect, IEnumerable<string> orderClause, IEnumerable<string> selectNames, string parNameSkip, string parNameTake);
   }

   public static class SqlFormatterExtension
   {
      public static string FormatIdentifier(this ISqlFormatter self, params string[] identifiers)
      {
         using (var sw = new StringWriter())
         {
            self.FormatIdentifier(sw, identifiers);
            return sw.GetStringBuilder().ToString();
         }
      }
      public static ISqlFormatter FormatIdentifier(this ISqlFormatter self, TextWriter w, params string[] identifiers)
      {
         string sep = null;
         foreach (var i in identifiers)
         {
            if (i == null) continue;
            w.Write(sep);
            var delimited = i.StartsWith(self.IdentifierDelimiterLeft);
            if (!delimited)
               w.Write(self.IdentifierDelimiterLeft);
            w.Write(i);
            if (!delimited)
               w.Write(self.IdentifierDelimiterRight);
            sep = sep ?? self.IdentifierSeperator;
         }
         return self;
      }
   }


}