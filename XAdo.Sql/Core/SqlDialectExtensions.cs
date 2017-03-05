using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using XAdo.Quobs.Core.Expressions;
using XAdo.Quobs.Core.Interface;
using XPression.Core;

namespace XAdo.Quobs.Core
{
   public static class SqlDialectExtensions
   {
      private static readonly HashSet<string>
         InitializedSet = new HashSet<string>();

      private static readonly ConcurrentDictionary<string, HashSet<string>>
         Aggregates = new ConcurrentDictionary<string, HashSet<string>>();

      public static HashSet<string> GetAggregates(this ISqlDialect self)
      {
         return Aggregates.GetOrAdd(self.ProviderName,
            n => new HashSet<string>(self.Aggregates, new StringComparerOrdinalIgnoreCase()));
      } 

      public static ISqlDialect EnsureAnnotated(this ISqlDialect self)
      {
         lock (InitializedSet)
         {
            if (InitializedSet.Contains(self.ProviderName)) return self;
            InitializedSet.Add(self.ProviderName);
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
            member.Annotate(new SqlFormatAttribute(format,self.ProviderName));
         }
      }

   }
}