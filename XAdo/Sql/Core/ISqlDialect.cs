using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XAdo.Sql.Core
{
   public interface ISqlDialect
   {
      void FormatValue(TextWriter writer, object value);

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

      string Exists { get; }
      string TypeCast { get; }
      string Coalesce { get; }
      string Modulo { get; }
      string Power { get; }

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

      IDictionary<Type, string> TypeMap { get; }

   }

   public static class SqlDialectExtension
   {
      private static readonly HashSet<Type> 
         InitializedSet = new HashSet<Type>();

      public static ISqlDialect EnsureAnnotated(this ISqlDialect self)
      {
         lock (InitializedSet)
         {
            if (InitializedSet.Contains(self.GetType())) return self;
            InitializedSet.Add(self.GetType());
         }

         //todo: enum/flag handling
         self.Annotate(KnownMembers.String.Length, self.StringLength);
         self.Annotate(KnownMembers.String.Contains, self.StringContains);
         self.Annotate(KnownMembers.String.StartsWith, self.StringStartsWith);
         self.Annotate(KnownMembers.String.EndsWith, self.StringEndsWith);
         self.Annotate(KnownMembers.String.ToLower, self.StringToLower);
         self.Annotate(KnownMembers.String.ToUpper, self.StringToUpper);
         self.Annotate(KnownMembers.String.Equals, "({0} = {1})");
         self.Annotate(KnownMembers.String.EqualsStatic, "({0} = {1})");


         self.Annotate(KnownMembers.DateTime.AddDays, self.DateTimeAddDays);
         self.Annotate(KnownMembers.DateTime.AddHours, self.DateTimeAddHours);
         self.Annotate(KnownMembers.DateTime.AddMilliseconds, self.DateTimeAddMilliSeconds);
         self.Annotate(KnownMembers.DateTime.AddMinutes, self.DateTimeAddMinutes);
         self.Annotate(KnownMembers.DateTime.AddMonths, self.DateTimeAddMonths);
         self.Annotate(KnownMembers.DateTime.AddSeconds, self.DateTimeAddSeconds);
         self.Annotate(KnownMembers.DateTime.AddYears, self.DateTimeAddYears);

         self.Annotate(KnownMembers.DateTime.Date, self.DateTimeGetDate);
         self.Annotate(KnownMembers.DateTime.Day, self.DateTimeGetDay);
         self.Annotate(KnownMembers.DateTime.DayOfWeek, self.DateTimeGetDayOfWeek);
         self.Annotate(KnownMembers.DateTime.DayOfYear, self.DateTimeGetDayOfYear);
         self.Annotate(KnownMembers.DateTime.Hour, self.DateTimeGetHour);
         self.Annotate(KnownMembers.DateTime.Millisecond, self.DateTimeGetMilliSecond);

         self.Annotate(KnownMembers.DateTime.Minute, self.DateTimeGetMinute);
         self.Annotate(KnownMembers.DateTime.Month, self.DateTimeGetMonth);
         self.Annotate(KnownMembers.DateTime.Now, self.DateTimeNow);
         self.Annotate(KnownMembers.DateTime.Second, self.DateTimeGetSecond);
         self.Annotate(KnownMembers.DateTime.Today, self.DateTimeToday);
         self.Annotate(KnownMembers.DateTime.UtcNow, self.DateTimeUtcNow);
         self.Annotate(KnownMembers.DateTime.Year, self.DateTimeGetYear);

         self.Annotate(KnownMembers.Math.CeilingDecimal, self.MathCeiling);
         self.Annotate(KnownMembers.Math.CeilingDouble, self.MathCeiling);
         self.Annotate(KnownMembers.Math.FloorDecimal, self.MathFloor);
         self.Annotate(KnownMembers.Math.FloorDouble, self.MathFloor);
         self.Annotate(KnownMembers.Math.RoundDecimal, self.MathRound);
         self.Annotate(KnownMembers.Math.RoundDecimalZeroDigits, self.MathRoundZeroDecimals);
         self.Annotate(KnownMembers.Math.RoundDouble, self.MathRound);
         self.Annotate(KnownMembers.Math.RoundDoubleZeroDigits, self.MathRoundZeroDecimals);

         return self;
      }

      private static void Annotate(this ISqlDialect self, MemberInfo member, string format)
      {
         if (!string.IsNullOrEmpty(format))
         {
            member.Annotate(new SqlFormatAttribute(format,self.GetType()));
         }
      }

   }
}