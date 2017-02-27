using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XAdo.Quobs.Linq
{
   public static class SqlMethods
   {

      #region Aggregates

      #region StDev

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("d => StDev", Order = 10)]// next: perform STDEV on casted value
      public static T StDev<T>(this byte value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("d => StDev", Order = 10)]// next: perform STDEV on casted value
      public static T StDev<T>(this int value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this long value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this float value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this double value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this decimal value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this byte? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this int? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this long? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this float? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this double? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDev", Order = 10)]
      public static T StDev<T>(this decimal? value) { throw NotImplemented(); }

      [SqlFormat("d => StDev")]
      public static T StDev<T>(this T value) { throw NotImplemented(); }

      [SqlFormat("d => StDev")]
      public static object StDev(object value) { throw NotImplemented(); }
      #endregion

      #region StDevP

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("d => StDevP", Order = 10)]// next: perform STDEV on casted value
      public static T StDevP<T>(this byte value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("d => StDevP", Order = 10)]// next: perform STDEV on casted value
      public static T StDevP<T>(this int value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this long value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this float value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this double value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this decimal value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this byte? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this int? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this long? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this float? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this double? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("d => StDevP", Order = 10)]
      public static T StDevP<T>(this decimal? value) { throw NotImplemented(); }

      [SqlFormat("d => StDevP")]
      public static T StDevP<T>(this T value) { throw NotImplemented(); }

      [SqlFormat("d => StDevP")]
      public static object StDevP(object value) { throw NotImplemented(); }
      #endregion

      #region Avg

      [SqlFormat("d => d.TypeCast",IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("AVG({0})", Order = 10)]// next: perform AVG on casted value
      public static T Avg<T>(this byte value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("AVG({0})", Order = 10)]// next: perform AVG on casted value
      public static T Avg<T>(this int value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this long value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this float value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this double value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this decimal value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this byte? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this int? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this long? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this float? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this double? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)]
      [SqlFormat("AVG({0})", Order = 10)]
      public static T Avg<T>(this decimal? value) { throw NotImplemented(); }

      [SqlFormat("AVG({0})")]
      public static T Avg<T>(this T value) { throw NotImplemented(); }

      [SqlFormat("AVG({0})")]
      public static object Avg(object value) { throw NotImplemented(); }
      #endregion

      #region Min

      [SqlFormat("MIN({0})")]
      public static byte Min(this byte value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static int Min(this int value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static long Min(this long value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static float Min(this float value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static double Min(this double value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static decimal Min(this decimal value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static DateTime Min(this DateTime value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static string Min(this string value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static byte? Min(this byte? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static int? Min(this int? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static long? Min(this long? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static float? Min(this float? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static double? Min(this double? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static decimal? Min(this decimal? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static DateTime? Min(this DateTime? value) { throw NotImplemented(); }

      [SqlFormat("MIN({0})")]
      public static object Min(object value) { throw NotImplemented(); }
      #endregion

      #region Max

      [SqlFormat("MAX({0})")]
      public static byte Max(this byte value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static int Max(this int value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static long Max(this long value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static float Max(this float value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static double Max(this double value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static decimal Max(this decimal value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static DateTime Max(this DateTime value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static string Max(this string value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static byte? Max(this byte? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static int? Max(this int? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static long? Max(this long? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static float? Max(this float? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static double? Max(this double? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static decimal? Max(this decimal? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static DateTime? Max(this DateTime? value) { throw NotImplemented(); }

      [SqlFormat("MAX({0})")]
      public static object Max(object value) { throw NotImplemented(); }


      #endregion

      #region Sum

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("SUM({0})",Order = 10)]
      public static T Sum<T>(this byte value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("SUM({0})", Order = 10)]
      public static T Sum<T>(this int value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("SUM({0})", Order = 10)]
      public static T Sum<T>(this byte? value) { throw NotImplemented(); }

      [SqlFormat("d => d.TypeCast", IncludeGenericParameters = true, Order = 1)] // first: perform typecast, using current dialect
      [SqlFormat("SUM({0})", Order = 10)]
      public static T Sum<T>(this int? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static byte Sum(this byte value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static int Sum(this int value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static long Sum(this long value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static float Sum(this float value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static double Sum(this double value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static decimal Sum(this decimal value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static byte? Sum(this byte? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static int? Sum(this int? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static long? Sum(this long? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static float? Sum(this float? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static double? Sum(this double? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static decimal? Sum(this decimal? value) { throw NotImplemented(); }

      [SqlFormat("SUM({0})")]
      public static object Sum(object value) { throw NotImplemented(); }


      #endregion

      #region Count

      [SqlFormat("COUNT({0})")]
      public static int Count(this object value) { throw NotImplemented(); }
      [SqlFormat("COUNT({0})")]
      public static int Count(this string value) { throw NotImplemented(); }

      #endregion

      #endregion


      [SqlFormat("x => x.DateTimeGetWeekNumber")]
      public static int? WeekNumber(this DateTime? self)
      {
         return self == null ? (int?)null : self.Value.WeekNumber();
      }

      [SqlFormat("x => x.DateTimeGetWeekNumber")]
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


      [SqlFormat("({0} IN ({1},...))")]
      public static bool In<T>(this T value, params T[] values)
      {
         return values.Contains(value);
      }
      [SqlFormat("x => x.StringConcat")]
      public static string Concat(this string value, params string[] values)
      {
         var sb = new StringBuilder(value);
         foreach (var v in values)
         {
            sb.Append(v);
         }
         return sb.ToString();
      }
      [SqlFormat("x => x.StringConcat")]
      public static string Concat(params string[] values)
      {
         var sb = new StringBuilder();
         foreach (var v in values)
         {
            sb.Append(v);
         }
         return sb.ToString();
      }

      [SqlFormat("({0} >= {1} AND {0} <= {2})")]
      public static bool Between<T>(this T value, T lower, T upper)
      {
         var val = (IComparable)value;

         return val.CompareTo(lower) >= 0 && val.CompareTo(upper) <= 0;
      }

      [SqlFormat("x => x.TypeCast", IncludeGenericParameters = true)]
      public static T DbConvert<T>(this object self)
      {
         return self == null ? default(T) : (T)self;
      }


      private static Exception NotImplemented()
      {
         return new NotImplementedException("This method is a XADO query method");
      }


   }
}
