using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sql.Parser.Common;

namespace Sql.Parser.Mapper
{
   public static class MapperExtensions
   {
      public static IDictionary<MemberInfo, string> GetMemberToFullNameMap(this Type type, IDictionary<MemberInfo, string> map = null, string path = null)
      {
         map = map ?? new Dictionary<MemberInfo, string>();
         path = path ?? "";
         foreach (var m in type.GetPropertiesAndFields())
         {
            if (map.ContainsKey(m)) return map;
            map[m] = (path + "." + m.Name).TrimStart('.');
            var t = m.GetMemberType();
            if (!t.IsScalarType())
            {
               t.GetMemberToFullNameMap(map, m.Name);
            }
         }
         return map;

      }

      public static IDictionary<string, MemberInfo> GetFullNameToMemberMap(this Type type)
      {
         return type.GetMemberToFullNameMap().ToDictionary(m => m.Value, m => m.Key, StringComparer.OrdinalIgnoreCase);
      }

   }
}
