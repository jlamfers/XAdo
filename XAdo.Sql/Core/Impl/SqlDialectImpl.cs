using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Core.Impl
{
   // JSON serializable class
   public class SqlDialectImpl : ISqlDialect
   {
      private string _literalTrue = "1";
      private string _literalFalse = "0";
      private string _literalNull = "NULL";
      private string[] _aggregates = { "AVG", "MIN", "MAX", "SUM", "COUNT" };


      public virtual void FormatValue(TextWriter writer, object value)
      {
         var self = (ISqlDialect)this;

         if (value == null)
         {
            writer.Write(LiteralNull);
            return;
         }

         switch (Type.GetTypeCode(value.GetType()))
         {
            case TypeCode.Char:
               var ch = (char)value;
               if (Char.IsControl(ch))
                  writer.Write(CharFormat, (int)ch);
               else
                  writer.Write("{0}{1}{2}", StringDelimiter, ch, StringDelimiter);
               return;
            case TypeCode.String:
               writer.Write("{0}{1}{0}", self.StringDelimiter, EscapeString((string)value));
               return;
            case TypeCode.DBNull:
            case TypeCode.Empty:
               writer.Write(LiteralNull);
               return;
            case TypeCode.Object:
               FormatValue(writer, value.ToString());
               return;
            case TypeCode.Boolean:
               writer.Write((bool)value ? LiteralTrue : LiteralFalse);
               return;
            case TypeCode.SByte:
               writer.Write(ToString<SByte>(value));
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
               FormatValue(writer, string.Format(self.DateTimeFormat, (DateTime)value));
               return;
         }
         throw new ArgumentOutOfRangeException();

      }

      public virtual string SelectTemplate { get; set; }

      public virtual string ProviderName { get; set; }

      public virtual string LiteralTrue { get { return _literalTrue; } set { _literalTrue = value; } }
      public virtual string LiteralFalse { get { return _literalFalse; } set { _literalFalse = value; } }
      public virtual string LiteralNull { get { return _literalNull; } set { _literalNull = value; } }

      public virtual string IdentifierSeperator { get; set; }
      public virtual string StatementSeperator { get; set; }
      public virtual string IdentifierDelimiterLeft { get; set; }
      public virtual string IdentifierDelimiterRight { get; set; }
      public virtual string StringDelimiter { get; set; }
      public virtual string EscapedStringDelimiter { get; set; }

      public virtual string ParameterFormat { get; set; }
      public virtual string DateTimeFormat { get; set; }
      public virtual string CharFormat { get; set; }
      public virtual string ExistsFormat { get; set; }
      public virtual string CountFormat { get; set; }


      public virtual string TypeCast { get; set; }
      public virtual string Coalesce { get; set; }
      public virtual string Modulo { get; set; }
      public virtual string Power { get; set; }
      public virtual string StDev { get; set; }
      public virtual string StDevP { get; set; }

      public virtual string StringLength { get; set; }
      public virtual string StringToUpper { get; set; }
      public virtual string StringToLower { get; set; }
      public virtual string StringContains { get; set; }
      public virtual string StringStartsWith { get; set; }
      public virtual string StringEndsWith { get; set; }
      public virtual string StringConcat { get; set; }

      public virtual string MathFloor { get; set; }
      public virtual string MathRound { get; set; }
      public virtual string MathRoundZeroDecimals { get; set; }
      public virtual string MathCeiling { get; set; }

      public virtual string DateTimeNow { get; set; }
      public virtual string DateTimeToday { get; set; }
      public virtual string DateTimeUtcNow { get; set; }
      public virtual string DateTimeAddDays { get; set; }
      public virtual string DateTimeAddMonths { get; set; }
      public virtual string DateTimeAddYears { get; set; }
      public virtual string DateTimeAddHours { get; set; }
      public virtual string DateTimeAddMinutes { get; set; }
      public virtual string DateTimeAddSeconds { get; set; }
      public virtual string DateTimeAddMilliSeconds { get; set; }

      public virtual string DateTimeGetDay { get; set; }
      public virtual string DateTimeGetMonth { get; set; }
      public virtual string DateTimeGetYear { get; set; }
      public virtual string DateTimeGetHour { get; set; }
      public virtual string DateTimeGetMinute { get; set; }
      public virtual string DateTimeGetSecond { get; set; }
      public virtual string DateTimeGetMilliSecond { get; set; }
      public virtual string DateTimeGetDate { get; set; }
      public virtual string DateTimeGetDayOfWeek { get; set; }
      public virtual string DateTimeGetDayOfYear { get; set; }
      public virtual string DateTimeGetWeekNumber { get; set; }

      public virtual string SelectLastIdentity { get; set; }
      public virtual string SelectLastIdentityTyped { get; set; }

      public virtual string BitwiseNot { get; set; }
      public virtual string BitwiseAnd { get; set; }
      public virtual string BitwiseOr { get; set; }
      public virtual string BitwiseXOR { get; set; }

      public virtual string[] Aggregates { get { return _aggregates; } set { _aggregates = value; } }

      public virtual IDictionary<Type, string> TypeMap { get; set; }

      protected virtual string EscapeString(string value)
      {
         var self = (ISqlDialect)this;
         if (string.IsNullOrWhiteSpace(value)) return value;
         return value.Replace(self.StringDelimiter, self.EscapedStringDelimiter);
      }

      protected virtual string ToString<T>(object value)
      {
         return string.Format(CultureInfo.InvariantCulture, "{0}", (T)value);
      }

   }
}

