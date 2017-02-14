using System;
using System.Globalization;
using System.Linq;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   public static class SqlMethods
   {

      [SqlFormat("dialect => dialect.DateTimeGetWeekNumber")]
      public static int? WeekNumber(this DateTime? self)
      {
         return self == null ? (int?)null : self.Value.WeekNumber();
      }

      [SqlFormat("dialect => dialect.DateTimeGetWeekNumber")]
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

      [SqlFormat("({0} >= {1} AND {0} <= {2})")]
      public static bool Between<T>(this T value, T lower, T upper)
      {
         var val = (IComparable) value;

         return val.CompareTo(lower) >= 0 && val.CompareTo(upper) <= 0;
      }

      [SqlFormat("dialect => dialect.TypeCast")]
      public static T Convert<T>(this object self)
      {
         return self == null ? default(T) : (T)self;
      }

      [SqlFormat("{0}")]
      public static CString AsComparable(this string self)
      {
         return new CString(self);
      }

   }
}
