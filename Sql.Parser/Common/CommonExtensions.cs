using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sql.Parser.Parser;

namespace Sql.Parser.Common
{
   internal static class CommonExtensions
   {
      public static bool IsNullable(this Type self)
      {
         return self != null && Nullable.GetUnderlyingType(self) != null;
      }
      public static bool IsScalarType(this Type self)
      {
         return self.IsPrimitive || self.IsValueType || (self == typeof(string)) || self == typeof(byte[]);
      }

      public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> self)
      {
         return new ReadOnlyDictionary<TKey, TValue>(self);
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

      public static string TrimQuotes(this string self)
      {
         if (string.IsNullOrEmpty(self)) return self;
         var left = self[0];
         char right;
         if (Scanner.Quotes.TryGetValue(left, out right) && self.Last() == right)
         {
            return self.Substring(1, self.Length - 2);
         }
         return self;
      }


   }
}