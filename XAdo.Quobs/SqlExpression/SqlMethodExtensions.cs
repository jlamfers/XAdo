using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core.SqlExpression
{
   public static class SqlMethodExtensions
   {

      #region Aggregates
      #region Avg

      [SqlAggregate.Avg]
      public static T Avg<T>(this byte value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this int value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this long value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this float value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this double value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this decimal value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this byte? value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this int? value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this long? value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this float? value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this double? value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this decimal? value) { throw NotImplemented(); }

      [SqlAggregate.Avg]
      public static T Avg<T>(this T value) { throw NotImplemented(); }
      #endregion

      #region Min

      [SqlAggregate.Min]
      public static byte Min(this byte value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static int Min(this int value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static long Min(this long value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static float Min(this float value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static double Min(this double value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static decimal Min(this decimal value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static DateTime Min(this DateTime value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static string Min(this string value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static byte? Min(this byte? value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static int? Min(this int? value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static long? Min(this long? value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static float? Min(this float? value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static double? Min(this double? value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static decimal? Min(this decimal? value) { throw NotImplemented(); }

      [SqlAggregate.Min]
      public static DateTime? Min(this DateTime? value) { throw NotImplemented(); }

      #endregion

      #region Max

      [SqlAggregate.Max]
      public static byte Max(this byte value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static int Max(this int value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static long Max(this long value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static float Max(this float value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static double Max(this double value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static decimal Max(this decimal value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static DateTime Max(this DateTime value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static string Max(this string value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static byte? Max(this byte? value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static int? Max(this int? value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static long? Max(this long? value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static float? Max(this float? value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static double? Max(this double? value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static decimal? Max(this decimal? value) { throw NotImplemented(); }

      [SqlAggregate.Max]
      public static DateTime? Max(this DateTime? value) { throw NotImplemented(); }

      #endregion

      #region Count

      [SqlAggregate.Count]
      public static int Count(this object value) { throw NotImplemented(); }
      [SqlAggregate.Count]
      public static int Count(this string value) { throw NotImplemented(); }

      #endregion
      #endregion

      [SqlWeekNumber]
      public static int? WeekNumber(this DateTime? self)
      {
         return self == null ? (int?)null : self.Value.WeekNumber();
      }

      [SqlWeekNumber]
      public static int WeekNumber(this DateTime self)
      {
         var cal = CultureInfo.InvariantCulture.Calendar;
         var day = cal.GetDayOfWeek(self);
         if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
         {
            self = self.AddDays(3);
         }
         return cal.GetWeekOfYear(self, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
      }

      [SqlIn]
      public static bool In<T>(this T value, params T[] values)
      {
         return values.Contains(value);
      }

      [SqlBetween]
      public static bool Between<T>(this T value, T lower, T upper)
      {
         throw NotImplemented();
      }

      [SqlCast]
      public static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T)self;
      }

      public static T DefaultIfEmpty<T>(this object self, Func<T> newExpression)
      {
         return self == null ? default(T) : newExpression();
      }
      public static TResult DefaultIfEmpty<T, TResult>(this T self, Func<T, TResult> newExpression)
      {
         throw NotImplemented();
      }
      public static TResult Create<T, TResult>(this T self, Func<T, TResult> newExpression)
      {
         throw NotImplemented();
      }

      [SqlIgnore]
      public static CString ToCString(this string self)
      {
         return new CString(self);
      }

      private static Exception NotImplemented()
      {
         return new NotImplementedException("This method is a quob query method");
      }

   }
}
