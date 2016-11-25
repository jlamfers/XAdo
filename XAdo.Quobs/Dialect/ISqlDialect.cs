using System;
using System.Collections.Generic;

namespace XAdo.Quobs.Dialect
{
   public interface ISqlDialect
   {
      string IdentifierSeperator { get; }
      string StatementSeperator { get; }
      string IdentifierDelimiterLeft { get; }
      string IdentifierDelimiterRight { get; }
      string StringDelimiter { get; }
      string EscapedStringDelimiter { get; }
      string ParameterPrefix { get; }
      string DateTimeFormat { get; }
      string Now { get; }
      string Today { get; }
      string UtcNow { get; }
      string Exists { get; }
      string TypeCast { get; }
      string Modulo { get; }
      string StringLength { get; }
      string ToUpper { get; }
      string ToLower { get; }
      string Floor { get; }
      string Round { get; }
      string Ceiling { get; }
      string Coalesce { get; }
      string Concat { get; }
      string DateTimeAddDay { get; }
      string DateTimeAddMonth { get; }
      string DateTimeAddYear { get; }
      string DateTimeAddHour { get; }
      string DateTimeAddMinute { get; }
      string DateTimeAddSecond { get; }
      string DateTimeAddMilliSecond { get; }
      string DateTimeGetDay { get; }
      string DateTimeGetMonth { get; }
      string DateTimeGetYear { get; }
      string DateTimeGetHour { get; }
      string DateTimeGetMinute { get; }
      string DateTimeGetSecond { get; }
      string DateTimeGetMilliSecond { get; }
      string DateTimeGetDate { get; }
      string DateTimeGetWeekDay { get; }
      string DateTimeGetDayOfYear { get; }
      string DateTimeGetWeekNumber { get; }
      string SelectLastIdentity { get; }
      IDictionary<Type, string> TypeMap { get; }
   }
}