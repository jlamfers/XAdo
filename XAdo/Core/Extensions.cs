﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core
{
    public static partial class Extensions
    {

       public static string FormatTemplate(this IXAdoDbSession self, string sqlTemplate, object argsObject)
       {
          return self.Context.GetInstance<ISqlTemplateFormatter>().Format(sqlTemplate, argsObject);
       }

       public static bool IsScalarType(this Type self)
       {
          return self.IsPrimitive || self.IsValueType || (self == typeof(string)) || self == typeof(byte[]);
       }

       public static IDictionary<MemberInfo, string> GetMemberToFullNameMap(this Type type, IDictionary<MemberInfo, string> map = null, string path = null)
       {
          map = map ?? new Dictionary<MemberInfo, string>();
          path = path ?? "";
          foreach (var m in type.GetPropertiesAndFields())
          {
             if (map.ContainsKey(m)) return map;
             var p = (path + "." + m.Name).TrimStart('.');
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

       internal static T CastTo<T>(this object self)
        {
            return self == null || self == DBNull.Value ? default(T) : (T) self;
        }

        internal static T CastTo<T>(this object self, IXAdoTypeConverterFactory typeConverterFactory)
        {

            if (self == null || self == DBNull.Value) return default(T);
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (type.IsInstanceOfType(self))
            {
                return (T)self;
            }
            var converter = typeConverterFactory.GetConverter<T>(self.GetType());
            return converter(self);
        }

        public static Type EnsureNotNullable(this Type self)
        {
            return self == null ? null : (Nullable.GetUnderlyingType(self) ?? self);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
            where T:class
        {
            var rows = data as IEnumerable<IEnumerable<KeyValuePair<string, object>>>;
            if (rows != null)
            {
                return rows.ToDataTable(null);
            }
            var elementType = typeof(T);
            var table = new DataTable();
            var props = elementType.GetProperties();
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType.EnsureNotNullable());
            }
            var values = new object[props.Length];
            var getters = props.Select(p => p.ToGetterSetter()).ToArray();
            foreach (var item in data)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = getters[i].Get(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        private static DataTable ToDataTable(this IEnumerable<IEnumerable<KeyValuePair<string, object>>> data, IEnumerable<KeyValuePair<string, Type>> columnTypeMap = null)
        {
            if (data == null) return null;
            var row = data.FirstOrDefault();
            var table = new DataTable();
            if (row == null)
            {
                if (columnTypeMap == null)
                {
                    throw new XAdoException("Dynamic data is empty (no rows). You must provide meta data (argument columnTypeMap) to be able to create a DataTable");
                };
                foreach (var kv in columnTypeMap)
                {
                    var type = kv.Value ?? typeof(object);
                    table.Columns.Add(kv.Key, type.EnsureNotNullable());
                }
            }
            else
            {
                foreach (var kv in row)
                {
                    var type = kv.Value == null ? typeof (object) : kv.Value.GetType();
                    table.Columns.Add(kv.Key, type.EnsureNotNullable());
                }
            }
            foreach (var item in data)
            {
                table.Rows.Add(item.Select(i => i.Value).ToArray());
            }
            return table;
        }

        public static CollectionDataReader ToDataReader<T>(this IEnumerable<T> enumerable)
            where T: class
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            var rows = enumerable as IEnumerable<IEnumerable<KeyValuePair<string, object>>>;
            return rows != null ? rows.ToDataReader(null) : new CollectionDataReader<T>(enumerable);
        }

        public static CollectionDataReader ToDataReader(this IEnumerable<IEnumerable<KeyValuePair<string, object>>> enumerable, IEnumerable<KeyValuePair<string, Type>> columnTypeMap = null)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            return new CollectionDataReader(enumerable, columnTypeMap);
        }

    }
}
