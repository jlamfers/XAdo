using System;
using System.Globalization;
using System.IO;

namespace XAdo.Sql.Core
{
   public class SqlFormatter : ISqlFormatter
   {

      public SqlFormatter(ISqlDialect dialect)
      {
         SqlDialect = dialect;
      }

      public ISqlDialect SqlDialect { get; private set; }

      public virtual void FormatParameter(TextWriter w, string parameterName)
      {
         if (string.IsNullOrEmpty(parameterName)) return;
         if (!parameterName.StartsWith(SqlDialect.ParameterPrefix))
         {
            w.Write(SqlDialect.ParameterPrefix);
         }
         w.Write(parameterName);
      }

      public virtual object NormalizeValue(object value)
      {
         return value;
      }

      public virtual void WriteExists(TextWriter writer, Action<TextWriter> sqlSelect)
      {
         SqlDialect.Exists.FormatSql(writer, sqlSelect);
      }
      public virtual void WriteTrue(TextWriter writer)
      {
         writer.Write("(1 = 1)");
      }
      public virtual void WriteFalse(TextWriter writer)
      {
         writer.Write("(1<>1)");
      }
      public virtual void WriteTypeCast(TextWriter writer, Type type, Action<TextWriter> value)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         string sqlType;
         if (!SqlDialect.TypeMap.TryGetValue(type, out sqlType))
         {
            value(writer);
            return;
         }
         SqlDialect.TypeCast.FormatSql(writer, value, w => FormatType(w,type));
      }
      public virtual void FormatType(TextWriter writer, Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         string sqlType;
         if (!SqlDialect.TypeMap.TryGetValue(type, out sqlType))
         {
            throw new Exception(string.Format("Type {0} not supported.", type));
         }
         writer.Write(sqlType);
      }
      public virtual void FormatValue(TextWriter writer, object value)
      {
         if (value == null)
         {
            writer.Write("NULL");
            return;
         }

         value = NormalizeValue(value);

         switch (Type.GetTypeCode(value.GetType()))
         {
            case TypeCode.Char:
               var ch = (char)value;
               if (Char.IsControl(ch))
                  writer.Write("CHAR({0})", (int) ch);
               else
                  writer.Write("'{0}'", ch);
               return;
            case TypeCode.String:
               writer.Write("{0}{1}{0}", SqlDialect.StringDelimiter, EscapeString((string)value));
               return;
            case TypeCode.DBNull:
            case TypeCode.Empty:
               writer.Write("NULL");
               return;
            case TypeCode.Object:
               FormatValue(writer,value.ToString());
               return;
            case TypeCode.Boolean:
               writer.Write((bool)value ? "1" : "0");
               return;
            case TypeCode.SByte:
               writer.Write( ToString<SByte>(value));
               return;
            case TypeCode.Byte:
               writer.Write(ToString<Byte>(value));
               return;
            case TypeCode.Int16:
               writer.Write(ToString<Int16>(value));
               return;
            case TypeCode.UInt16:
               writer.Write(ToString<UInt16>(value));
               return;
            case TypeCode.Int32:
               writer.Write(ToString<Int32>(value));
               return;
            case TypeCode.UInt32:
               writer.Write(ToString<UInt32>(value));
               return;
            case TypeCode.Int64:
               writer.Write(ToString<Int64>(value));
               return;
            case TypeCode.UInt64:
               writer.Write(ToString<UInt64>(value));
               return;
            case TypeCode.Single:
               writer.Write(ToString<Single>(value));
               return;
            case TypeCode.Double:
               writer.Write(ToString<Double>(value));
               return;
            case TypeCode.Decimal:
               writer.Write(ToString<Decimal>(value));
               return;
            case TypeCode.DateTime:
               FormatValue(writer, string.Format(SqlDialect.DateTimeFormat, (DateTime)value));
               return;
         }
         throw new ArgumentOutOfRangeException();
         
      }
      protected virtual string ToString<T>(object value)
      {
         return string.Format(CultureInfo.InvariantCulture, "{0}", (T)value);
      }
      protected virtual string EscapeString(string value)
      {
         if (string.IsNullOrWhiteSpace(value)) return value;
         return value.Replace(SqlDialect.StringDelimiter, SqlDialect.EscapedStringDelimiter);
      }
      public virtual void WriteConcatenate(TextWriter writer, params Action<TextWriter>[] args)
      {
         SqlDialect.Concat.FormatSql(writer,args);
      }
      public virtual void WriteCoalesce(TextWriter writer, params Action<TextWriter>[] args)
      {
         SqlDialect.Coalesce.FormatSql(writer,args);
      }
      public virtual void WriteDateTimeWeekNumber(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetWeekNumber.FormatSql(writer,date);
      }
      public virtual void WriteDateTimeDate(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetDate.FormatSql(writer,date);
      }
      public virtual void WriteDateTimeYear(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetYear.FormatSql(writer,date);
      }
      public virtual void WriteDateTimeMonth(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetMonth.FormatSql(writer, date);
         
      }
      public virtual void WriteDateTimeDay(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetDay.FormatSql(writer, date);
         
      }
      public virtual void WriteDateTimeDayOfWeek(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetWeekDay.FormatSql(writer, date);
         
      }
      public virtual void WriteDateTimeDayOfYear(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetDayOfYear.FormatSql(writer, date);
         
      }
      public virtual void WriteDateTimeHour(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetHour.FormatSql(writer, date);

      }
      public virtual void WriteDateTimeMinute(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetMinute.FormatSql(writer, date);

      }
      public virtual void WriteDateTimeSecond(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetSecond.FormatSql(writer, date);

      }
      public virtual void WriteDateTimeMillisecond(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetMilliSecond.FormatSql(writer, date);

      }
      public virtual void WriteDateTimeAddDays(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddDay.FormatSql(writer, date, count);
      }
      public virtual void WriteDateTimeAddYears(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddYear.FormatSql(writer, date, count);
         
      }
      public virtual void WriteDateTimeAddMonths(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddMonth.FormatSql(writer, date, count);
      }
      public virtual void WriteDateTimeAddHours(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddHour.FormatSql(writer, date, count);
      }
      public virtual void WriteDateTimeAddMinutes(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddMinute.FormatSql(writer, date, count);
      }
      public virtual void WriteDateTimeAddSeconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddSecond.FormatSql(writer, date, count);
      }
      public virtual void WriteDateTimeAddMilliseconds(TextWriter writer, Action<TextWriter> date,Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddMilliSecond.FormatSql(writer, date, count);
      }
      public virtual void WriteStringLength(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.StringLength.FormatSql(writer,arg);
      }
      public virtual void WriteToUpper(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.ToUpper.FormatSql(writer,arg);
      }
      public virtual void WriteToLower(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.ToLower.FormatSql(writer,arg);
      }
      public virtual void WriteFloor(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.Floor.FormatSql(writer,arg);
      }
      public virtual void WriteRound(TextWriter writer, Action<TextWriter> arg, Action<TextWriter> length)
      {
         SqlDialect.Round.FormatSql(writer,arg,length);
      }
      public virtual void WriteCeiling(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.Ceiling.FormatSql(writer,arg);
      }

      public virtual void WriteSelectLastIdentity(TextWriter writer)
      {
         writer.Write(SqlDialect.SelectLastIdentity);
      }
      public virtual void WriteSelectLastIdentity(TextWriter writer, Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         string sqlType;
         if (!SqlDialect.TypeMap.TryGetValue(type, out sqlType))
         {
            throw new Exception("Cannot format type " + type + " with SqlFormatter " + GetType());
         }
         writer.Write(SqlDialect.SelectLastIdentityTyped, sqlType);
      }

      public virtual void WriteCount(TextWriter writer, string select)
      {
         writer.Write("SELECT COUNT(1) FROM (");
         writer.Write(select);
         writer.Write(") AS __count_table");
      }

   }

}
