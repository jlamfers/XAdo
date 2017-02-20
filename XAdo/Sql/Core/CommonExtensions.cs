using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using XAdo.Core;

namespace XAdo.Sql.Core
{
   internal static class CommonExtensions
   {
      public static object GetValue(this MemberInfo member, object target)
      {
         var pi = member as PropertyInfo;
         return pi != null ? pi.GetValue(target) : ((FieldInfo)member).GetValue(target);
      }

      public static bool IsNullable(this Type self)
      {
         return self != null && Nullable.GetUnderlyingType(self) != null;
      }

      public static bool IsRuntimeGenerated(this Type self)
      {
         return Attribute.IsDefined(self, typeof(CompilerGeneratedAttribute), false);
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

      public static bool IsPrimitiveType(this Type self)
      {
         return self.IsPrimitive || self.IsValueType || (self == typeof(string)) || self == typeof(byte);
      }
      public static object CreateInstance(this Type self)
      {
         return Activator.CreateInstance(self);
      }

      public static IDictionary<MemberInfo, string> GetMemberToFullNameMap(this Type type, IDictionary<MemberInfo, string> map = null, string path=null)
      {
         map = map ?? new Dictionary<MemberInfo, string>();
         path = path ?? "";
         foreach (var m in type.GetPropertiesAndFields())
         {
            if (map.ContainsKey(m)) return map;
            map[m] = (path + "." + m.Name).TrimStart('.');
            var t = m.GetMemberType();
            if (!t.IsPrimitiveType())
            {
               t.GetMemberToFullNameMap(map, m.Name);
            }
         }
         return map;

      }
      public static IDictionary<string,MemberInfo> GetFullNameToMemberMap(this Type type)
      {
         return type.GetMemberToFullNameMap().ToDictionary(m => m.Value, m => m.Key, StringComparer.OrdinalIgnoreCase);
      }
   }
}