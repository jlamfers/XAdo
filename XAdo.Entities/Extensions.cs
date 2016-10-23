using System;

namespace XAdo.Quobs
{
   public static class Extensions
   {
      internal static T EnsureNotNull<T>(this T value, string name)
         where T : class
      {
         if (value == null)
         {
            throw new ArgumentNullException(name);
         }
         return value;
      }

      public static bool IsNullable(this Type self)
      {
         return self != null && Nullable.GetUnderlyingType(self) != null;
      }

      public static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T) self;
      }

      public static string FormatWith(this string self, params object[] args)
      {
         return string.Format(self, args);
      }

      public static string FormatWith(this string self, IFormatProvider provider, params object[] args)
      {
         return string.Format(provider, self, args);
      }
   }
}
