using System;
using System.Globalization;
using System.IO;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Dialect
{
    public class Ms2012SqlFormatter : BaseSqlFormatter
    {
        private static readonly DateTime MinDate = new DateTime(1753,1,1);

       public Ms2012SqlFormatter()
       {
          IdentifierDelimiterLeft = "[";
          IdentifierDelimiterRight = "]";
       }

       public override object NormalizeValue(object value)
        {
            return value == null || Type.GetTypeCode(value.GetType()) != TypeCode.DateTime
                ? value
                : ((DateTime)value == DateTime.MinValue ? MinDate : value);
        }

       public override void WriteExists(TextWriter writer, Action<TextWriter> sqlSelect)
       {
          writer.Write(@"SELECT CAST( CASE WHEN EXISTS(");
          sqlSelect(writer);
          writer.Write(@") THEN 1 ELSE 0 END AS BIT)");
 
       }

       public override void WriteTypeCast(TextWriter writer, Type type, Action<TextWriter> value)
       {
          writer.Write("CAST(");
          value(writer);
          writer.Write(" AS ");
          writer.Write(FormatType(type));
          writer.Write(")");
       }

       public override string DateTimeFormat { get { return "{0:yyyy-MM-dd HH:mm:ss.fff}"; } }
       public override string Now { get { return "GETDATE()"; } }
       public override string Today { get { return "CONVERT(DATE, GETDATE())"; } }
       public override string UtcNow { get { return "GETUTCDATE()"; } }

       public override string FormatValue(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Char:
                    var ch = (char) value;
                    return Char.IsControl(ch) 
                        ? string.Format("CHAR({0})", (int)ch) 
                        : string.Format("'{0}'",ch);
                case TypeCode.String:
                    return string.Format("'{0}'",((string)value).Replace("'","''"));
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    return "NULL";
                case TypeCode.Object:
                    return FormatValue(value.ToString());
                case TypeCode.Boolean:
                    return (bool) value ? "1" : "0";
                case TypeCode.SByte:
                    return ToString<SByte>(value);
                case TypeCode.Byte:
                    return ToString<Byte>(value);
                case TypeCode.Int16:
                    return ToString<Int16>(value);
                case TypeCode.UInt16:
                    return ToString<UInt16>(value);
                case TypeCode.Int32:
                    return ToString<Int32>(value);
                case TypeCode.UInt32:
                    return ToString<UInt32>(value);
                case TypeCode.Int64:
                    return ToString<Int64>(value);
                case TypeCode.UInt64:
                    return ToString<UInt64>(value);
                case TypeCode.Single:
                    return ToString<Single>(value);
                case TypeCode.Double:
                    return ToString<Double>(value);
                case TypeCode.Decimal:
                    return ToString<Decimal>(value);
                case TypeCode.DateTime:
                    var dt = (DateTime) value;
                    if (dt == DateTime.MinValue)
                    {
                        dt = MinDate;
                    }
                    return FormatValue(string.Format(DateTimeFormat, dt));
            }
            throw new ArgumentOutOfRangeException();
        }

       public override string FormatType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(Guid)) return "UNIQUEIDENTIFIER";
            if (type == typeof(bool)) return "BIT";
            if (type == typeof(byte)) return "TINYINT";
            if (type == typeof(sbyte)) return "SMALLINT";
            if (type == typeof(short)) return "SMALLINT";
            if (type == typeof(ushort)) return "INT";
            if (type == typeof(int)) return "INT";
            if (type == typeof(uint)) return "BIGINT";
            if (type == typeof(long)) return "BIGINT";
            if (type == typeof(ulong)) return "DECIMAL(20)";
            if (type == typeof(decimal)) return "DECIMAL(29,4)";
            if (type == typeof(float)) return "REAL";
            if (type == typeof(double)) return "FLOAT";
            if (type == typeof(char)) return "CHAR(1)";
            if (type == typeof(string)) return "NVARCHAR(MAX)";
            if (type == typeof(DateTime)) return "DATETIME";
            if (type == typeof(DateTimeOffset)) return "DATETIMEOFFSET";
            if (type == typeof(TimeSpan)) return "TIME";
            if (type == typeof(byte[])) return "VARBINARY(MAX)";
            throw new QuobException(string.Format("Type {0} not supported.", type));
        }


        private static string ToString<T>(object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", (T)value);
        }

        public override void WriteModulo(TextWriter writer, Action<TextWriter> left, Action<TextWriter> right)
        {
            writer.Write("(");
            left(writer);
            writer.Write(" % ");
            right(writer);
            writer.Write(")");
        }
        public override void WriteStringLength(TextWriter writer, Action<TextWriter> arg)
        {
            writer.Write("LEN(");
            arg(writer);
            writer.Write(")");
        }

        public override void WriteToUpper(TextWriter writer, Action<TextWriter> arg)
        {
            writer.Write("UPPER(");
            arg(writer);
            writer.Write(")");
        }

        public override void WriteToLower(TextWriter writer, Action<TextWriter> arg)
        {
            writer.Write("LOWER(");
            arg(writer);
            writer.Write(")");
        }

        public override void WriteFloor(TextWriter writer, Action<TextWriter> arg)
        {
            writer.Write("FLOOR(");
            arg(writer);
            writer.Write(")");
        }

        public override void WriteRound(TextWriter writer, Action<TextWriter> arg, Action<TextWriter> length)
        {
            writer.Write("ROUND(");
            arg(writer);
            writer.Write(",");
            length(writer);
            writer.Write(")");
        }

        public override void WriteCeiling(TextWriter writer, Action<TextWriter> arg)
        {
            writer.Write("CEILING(");
            arg(writer);
            writer.Write(")");
        }

        public override void WriteCoalesce(TextWriter writer, params Action<TextWriter>[] args)
        {
            writer.Write("COALESCE(");
            var comma = "";
            foreach (var arg in args)
            {
                writer.Write(comma);
                arg(writer);
                comma = ", ";
            }
            writer.Write(")");
        }

        public override void WriteConcatenate(TextWriter writer, params Action<TextWriter>[] args)
        {
            writer.Write("CONCAT(");
            var comma = "";
            foreach (var arg in args)
            {
                writer.Write(comma);
                arg(writer);
                comma = ", ";
            }
            writer.Write(")");
        }

        protected virtual void FormatDateTimeAdd(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count, string kind)
        {
            writer.Write("DATEADD(");
            writer.Write(kind);
            writer.Write(",");
            count(writer);
            writer.Write(",");
            date(writer);
            writer.Write(")");
        }
        public override void WriteDateTimeAddDays(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer,date,count,"DAY");
        }
        public override void WriteDateTimeAddYears(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer, date, count, "YEAR");
        }
        public override void WriteDateTimeAddMonths(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer, date, count, "MONTH");
        }
        public override void WriteDateTimeAddHours(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer, date, count, "HOUR");
        }
        public override void WriteDateTimeAddMinutes(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer, date, count, "MINUTE");
        }
        public override void WriteDateTimeAddSeconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer, date, count, "SECOND");
        }
        public override void WriteDateTimeAddMilliseconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
        {
            FormatDateTimeAdd(writer, date, count, "MILLISECOND");
        }

        protected virtual void FormatDatePart(TextWriter writer, string datepart, Action<TextWriter> date)
        {
            writer.Write("DATEPART(");
            writer.Write(datepart);
            writer.Write(", ");
            date(writer);
            writer.Write(")");
        }
        public override void WriteDateTimeDate(TextWriter writer, Action<TextWriter> date)
        {
            writer.Write("CONVERT(DATE,");
            date(writer);
            writer.Write(")");
        }
        public override void WriteDateTimeYear(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "YEAR", date);
        }
        public override void WriteDateTimeMonth(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "MONTH", date);
        }
        public override void WriteDateTimeDay(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "DAY", date);
        }
        public override void WriteDateTimeDayOfWeek(TextWriter writer, Action<TextWriter> date)
        {
            writer.Write("(");
            FormatDatePart(writer,"WEEKDAY",date);
            writer.Write("-1");// => compliant with DayOfWeek enumeration
            writer.Write(")");
        }
        public override void WriteDateTimeDayOfYear(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "DAYOFYEAR", date);
        
        }
        public override void WriteDateTimeHour(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "HOUR", date);
        }
        public override void WriteDateTimeMinute(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "MINUTE", date);
        }
        public override void WriteDateTimeSecond(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "SECOND", date);
        }
        public override void WriteDateTimeMillisecond(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "MILLISECOND", date);
        }
        public override void WriteDateTimeWeekNumber(TextWriter writer, Action<TextWriter> date)
        {
            FormatDatePart(writer, "ISOWK", date);
        }

    }
}
