using System;
using System.IO;

namespace XAdo.Sql.Core
{
   public interface ISqlFormatter
   {
      ISqlDialect SqlDialect { get; }
      void FormatParameter(TextWriter w, string parameterName);
      object NormalizeValue(object value);
      void WriteExists(TextWriter writer, Action<TextWriter> sqlSelect);
      void WriteTrue(TextWriter writer);
      void WriteFalse(TextWriter writer);
      void WriteTypeCast(TextWriter writer, Type type, Action<TextWriter> value);
      void FormatType(TextWriter writer, Type type);
      void FormatValue(TextWriter writer, object value);
      void WriteConcatenate(TextWriter writer, params Action<TextWriter>[] args);
      void WriteCoalesce(TextWriter writer, params Action<TextWriter>[] args);
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
      void WriteDateTimeAddMilliseconds(TextWriter writer, Action<TextWriter> date,Action<TextWriter> count);
      void WriteStringLength(TextWriter writer, Action<TextWriter> arg);
      void WriteToUpper(TextWriter writer, Action<TextWriter> arg);
      void WriteToLower(TextWriter writer, Action<TextWriter> arg);
      void WriteFloor(TextWriter writer, Action<TextWriter> arg);
      void WriteRound(TextWriter writer, Action<TextWriter> arg, Action<TextWriter> length);
      void WriteCeiling(TextWriter writer, Action<TextWriter> arg);
      void WriteSelectLastIdentity(TextWriter writer);
      void WriteSelectLastIdentity(TextWriter writer, Type type);
   }

   public static class SqlFormatterExtension
   {
      public static string FormatParameter(this ISqlFormatter self, string parameterName)
      {
         using (var w = new StringWriter())
         {
            self.FormatParameter(w, parameterName);
            return w.GetStringBuilder().ToString();
         }
      }
      public static string FormatValue(this ISqlFormatter self, object value)
      {
         using (var sw = new StringWriter())
         {
            self.FormatValue(sw, value);
            return sw.GetStringBuilder().ToString();
         }
      }
   }
}