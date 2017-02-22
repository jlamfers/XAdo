using System;
using System.Reflection;
using Sql.Parser.Common;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Sql.Parser.Linq
{
   internal static class KnownMembers
   {
      public static class String
      {
         public static readonly MethodInfo
             StartsWith = MemberInfoFinder.GetMethodInfo<string>(s => s.StartsWith("")),
             EndsWith = MemberInfoFinder.GetMethodInfo<string>(s => s.EndsWith("")),
             Contains = MemberInfoFinder.GetMethodInfo<string>(s => s.Contains("")),
             Compare = MemberInfoFinder.GetMethodInfo(() => string.Compare("", "")),
             CompareTo = MemberInfoFinder.GetMethodInfo<string>(s => s.CompareTo("")),
             Equals = MemberInfoFinder.GetMethodInfo<string>(s => s.Equals("")),
             EqualsStatic = MemberInfoFinder.GetMethodInfo(() => string.Equals("", "")),
             ToUpper = MemberInfoFinder.GetMethodInfo<string>(s => s.ToUpper()),
             ToLower = MemberInfoFinder.GetMethodInfo<string>(s => s.ToLower()),
             ToCString = MemberInfoFinder.GetMethodInfo<string>(s => s.AsComparable());

         public static readonly PropertyInfo
             Length = MemberInfoFinder.GetPropertyInfo<string>(s => s.Length);

      }

      public static class DateTime
      {
         public static readonly MethodInfo
             AddMilliseconds = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddMilliseconds(0)),
             AddSeconds = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddSeconds(0)),
             AddMinutes = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddMinutes(0)),
             AddHours = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddHours(0)),
             AddDays = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddDays(0)),
             AddMonths = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddMonths(0)),
             AddYears = MemberInfoFinder.GetMethodInfo<System.DateTime>(x => x.AddYears(0));

         public static readonly PropertyInfo
             Now = MemberInfoFinder.GetPropertyInfo(() => System.DateTime.Now),
             UtcNow = MemberInfoFinder.GetPropertyInfo(() => System.DateTime.UtcNow),
             Today = MemberInfoFinder.GetPropertyInfo(() => System.DateTime.Today),
             Date = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Date),
             Year = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Year),
             Month = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Month),
             Day = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Day),
             DayOfWeek = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.DayOfWeek),
             DayOfYear = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.DayOfYear),
             Hour = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Hour),
             Minute = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Minute),
             Second = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Second),
             Millisecond = MemberInfoFinder.GetPropertyInfo<System.DateTime>(d => d.Millisecond);

      }

      public static class Math
      {
         public static readonly MethodInfo
             RoundDouble = MemberInfoFinder.GetMethodInfo(() => System.Math.Round((double)0, 0)),
             RoundDecimal = MemberInfoFinder.GetMethodInfo(() => System.Math.Round((Decimal)0, 0)),
             RoundDoubleZeroDigits = MemberInfoFinder.GetMethodInfo(() => System.Math.Round((double)0)),
             RoundDecimalZeroDigits = MemberInfoFinder.GetMethodInfo(() => System.Math.Round((Decimal)0)),
             FloorDouble = MemberInfoFinder.GetMethodInfo(() => System.Math.Floor((double)0)),
             FloorDecimal = MemberInfoFinder.GetMethodInfo(() => System.Math.Floor((Decimal)0)),
             CeilingDouble = MemberInfoFinder.GetMethodInfo(() => System.Math.Ceiling((double)0)),
             CeilingDecimal = MemberInfoFinder.GetMethodInfo(() => System.Math.Ceiling((Decimal)0));
      }

      public static bool EqualMethods(this MethodInfo self, MethodInfo other)
      {
         if (self == other) return true;
         if (self.Name != other.Name && self.GetParameters().Length != other.GetParameters().Length || (self.IsGenericMethod != other.IsGenericMethod)) return false;
         if (!self.IsGenericMethod) return false;
         return self.GetGenericMethodDefinition() == other.GetGenericMethodDefinition();
      }
   }

}