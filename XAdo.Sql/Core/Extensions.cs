using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XAdo.Quobs.Core.Common
{
   internal static class Extensions
   {
      public static T CloneOrElseSelf<T>(this T self)
      {
         var cloneable = self as ICloneable;
         return cloneable != null ? cloneable.Clone().CastTo<T>() : self;
      }

      public static bool IsNullable(this Type self)
      {
         return self != null && Nullable.GetUnderlyingType(self) != null;
      }
      public static bool IsScalarType(this Type self)
      {
         return self.IsPrimitive || self.IsValueType || (self == typeof(string)) || self == typeof(byte[]);
      }
      //public static Type EnsureNotNullable(this Type self)
      //{
      //   return self == null ? null : (Nullable.GetUnderlyingType(self) ?? self);
      //}
      public static Type EnsureNullable(this Type self)
      {
         return self == null || !self.IsValueType || Nullable.GetUnderlyingType(self)  != null ? self : typeof(Nullable<>).MakeGenericType(self);
      }
      public static bool EqualsOrdinalIgnoreCase(this string self, string other)
      {
         return string.Equals(self, other, StringComparison.OrdinalIgnoreCase);
      }


      public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> self)
      {
         return new ReadOnlyDictionary<TKey, TValue>(self);
      }
      public static ICollection<TValue> AsReadOnly<TValue>(this ICollection<TValue> self)
      {
         return new MyReadOnlyCollection<TValue>(self);
      }
      public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> other)
      {
         foreach (var kv in other)
         {
            self.Add(kv);
         }
         return self;
      }

      public static string FormatWith(this string format, params object[] args)
      {
         return format == null ? null : string.Format(format, args);
      }

      public static object CreateInstance(this Type self)
      {
         return Activator.CreateInstance(self);
      }
      public static T CastTo<T>(this object self)
      {
         return self == null || self == DBNull.Value ? default(T) : (T)self;
      }

   }
}