using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Parser;

namespace XAdo.Quobs.Core.Mapper
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
            var p = (path + Constants.Syntax.Chars.NAME_SEP_STR + m.Name).TrimStart(Constants.Syntax.Chars.NAME_SEP);
            var t = m.GetMemberType();
            if (!t.IsScalarType())
            {
               t.GetMemberToFullNameMap(map, p);
            }
            //map[m] = p;
            else
            {
               map[m] = p;
            }
         }
         return map;

      }

      public static IDictionary<string, MemberInfo> GetFullNameToMemberMap(this Type type)
      {
         var dict = type.GetMemberToFullNameMap().ToDictionary(m => m.Value, m => m.Key, StringComparer.OrdinalIgnoreCase);
         return dict;
      }

   }
}
