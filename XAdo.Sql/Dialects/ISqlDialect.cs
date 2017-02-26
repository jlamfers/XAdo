using System;
using System.Collections.Generic;
using System.IO;

namespace XAdo.Sql.Dialects
{
   public interface ISqlDialect
   {
      void FormatValue(TextWriter writer, object value);

      string SelectTemplate { get; }

      string ProviderName { get; }

      string LiteralTrue { get; }
      string LiteralFalse { get; }
      string LiteralNull { get; }

      string IdentifierSeperator { get; }
      string StatementSeperator { get; }
      string IdentifierDelimiterLeft { get; }
      string IdentifierDelimiterRight { get; }
      string StringDelimiter { get; }
      string EscapedStringDelimiter { get; }

      string ParameterFormat { get; }
      string DateTimeFormat { get; }
      string CharFormat { get; }
      string ExistsFormat { get; }
      string CountFormat { get; }

      string TypeCast { get; }
      string Coalesce { get; }
      string Modulo { get; }
      string Power { get; }

      string StDev { get; }
      string StDevP { get; }

      string StringLength { get; }
      string StringToUpper { get; }
      string StringToLower { get; }
      string StringContains { get; }
      string StringStartsWith { get; }
      string StringEndsWith { get; }
      string StringConcat { get; }

      string MathFloor { get; }
      string MathRound { get; }
      string MathRoundZeroDecimals { get; }
      string MathCeiling { get; }


      string DateTimeNow { get; }
      string DateTimeToday { get; }
      string DateTimeUtcNow { get; }

      string DateTimeAddDays { get; }
      string DateTimeAddMonths { get; }
      string DateTimeAddYears { get; }
      string DateTimeAddHours { get; }
      string DateTimeAddMinutes { get; }
      string DateTimeAddSeconds { get; }
      string DateTimeAddMilliSeconds { get; }

      string DateTimeGetDay { get; }
      string DateTimeGetMonth { get; }
      string DateTimeGetYear { get; }
      string DateTimeGetHour { get; }
      string DateTimeGetMinute { get; }
      string DateTimeGetSecond { get; }
      string DateTimeGetMilliSecond { get; }
      string DateTimeGetDate { get; }
      string DateTimeGetDayOfWeek { get; }
      string DateTimeGetDayOfYear { get; }
      string DateTimeGetWeekNumber { get; }

      string SelectLastIdentity { get; }
      string SelectLastIdentityTyped { get; }

      string BitwiseNot { get; }
      string BitwiseAnd { get; }
      string BitwiseOr { get; }
      string BitwiseXOR { get; }
      string[] Aggregates { get; } 
      IDictionary<Type, string> TypeMap { get; }

   }
}