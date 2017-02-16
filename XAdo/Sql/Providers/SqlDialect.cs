using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace XAdo.Sql.Core
{
   // JSON serializable class
   public class SqlDialect : ISqlDialect
   {
      private string _literalTrue = "1";
      private string _literalFalse = "0";
      private string _literalNull = "NULL";

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

      public virtual string SelectTemplate { get; protected set; }

      public virtual string ProviderName { get; protected set; }

      public virtual string LiteralTrue { get { return _literalTrue; } protected set { _literalTrue = value; } }
      public virtual string LiteralFalse { get { return _literalFalse; } protected set { _literalFalse = value; } }
      public virtual string LiteralNull { get { return _literalNull; } protected set { _literalNull = value; } }

      public virtual string IdentifierSeperator { get; protected set; }
      public virtual string StatementSeperator { get; protected set; }
      public virtual string IdentifierDelimiterLeft { get; protected set; }
      public virtual string IdentifierDelimiterRight { get; protected set; }
      public virtual string StringDelimiter { get; protected set; }
      public virtual string EscapedStringDelimiter { get; protected set; }

      public virtual string ParameterFormat { get; protected set; }
      public virtual string DateTimeFormat { get; protected set; }
      public virtual string CharFormat { get; protected set; }
      public virtual string ExistsFormat { get; protected set; }
      public virtual string CountFormat { get; protected set; }


      public virtual string TypeCast { get; protected set; }
      public virtual string Coalesce { get; protected set; }
      public virtual string Modulo { get; protected set; }
      public virtual string Power { get; protected set; }

      public virtual string StringLength { get; protected set; }
      public virtual string StringToUpper { get; protected set; }
      public virtual string StringToLower { get; protected set; }
      public virtual string StringContains { get; protected set; }
      public virtual string StringStartsWith { get; protected set; }
      public virtual string StringEndsWith { get; protected set; }
      public virtual string StringConcat { get; protected set; }

      public virtual string MathFloor { get; protected set; }
      public virtual string MathRound { get; protected set; }
      public virtual string MathRoundZeroDecimals { get; protected set; }
      public virtual string MathCeiling { get; protected set; }

      public virtual string DateTimeNow { get; protected set; }
      public virtual string DateTimeToday { get; protected set; }
      public virtual string DateTimeUtcNow { get; protected set; }
      public virtual string DateTimeAddDays { get; protected set; }
      public virtual string DateTimeAddMonths { get; protected set; }
      public virtual string DateTimeAddYears { get; protected set; }
      public virtual string DateTimeAddHours { get; protected set; }
      public virtual string DateTimeAddMinutes { get; protected set; }
      public virtual string DateTimeAddSeconds { get; protected set; }
      public virtual string DateTimeAddMilliSeconds { get; protected set; }

      public virtual string DateTimeGetDay { get; protected set; }
      public virtual string DateTimeGetMonth { get; protected set; }
      public virtual string DateTimeGetYear { get; protected set; }
      public virtual string DateTimeGetHour { get; protected set; }
      public virtual string DateTimeGetMinute { get; protected set; }
      public virtual string DateTimeGetSecond { get; protected set; }
      public virtual string DateTimeGetMilliSecond { get; protected set; }
      public virtual string DateTimeGetDate { get; protected set; }
      public virtual string DateTimeGetDayOfWeek { get; protected set; }
      public virtual string DateTimeGetDayOfYear { get; protected set; }
      public virtual string DateTimeGetWeekNumber { get; protected set; }

      public virtual string SelectLastIdentity { get; protected set; }
      public virtual string SelectLastIdentityTyped { get; protected set; }

      public virtual string BitwiseNot { get; protected set; }
      public virtual string BitwiseAnd { get; protected set; }
      public virtual string BitwiseOr { get; protected set; }
      public virtual string BitwiseXOR { get; protected set; }

      public virtual IDictionary<Type, string> TypeMap { get; protected set; }

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

