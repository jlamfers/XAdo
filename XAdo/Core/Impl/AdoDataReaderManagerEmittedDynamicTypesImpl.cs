using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class AdoDataReaderManagerEmittedDynamicTypesImpl : AdoDataReaderManagerImpl
   {
      public AdoDataReaderManagerEmittedDynamicTypesImpl(IAdoDataBinderFactory binderFactory, IAdoGraphBinderFactory multiBinderFactory, IConcreteTypeBuilder proxyBuilder)
         : base(binderFactory, multiBinderFactory, proxyBuilder)
      {
      }

      private static readonly MethodInfo _readAll =
          typeof(IAdoDataReaderManager).GetMethods().Single(m => m.IsGenericMethod && m.GetGenericArguments().Length == 1 && m.Name == "ReadAll").GetGenericMethodDefinition();

      public override IEnumerable<dynamic> ReadAll(IDataReader reader)
      {
         if (reader == null) throw new ArgumentNullException("reader");
         if (reader.FieldCount == 1)
         {
            while (reader.Read())
            {
               yield return reader.IsDBNull(0) ? null : reader.GetValue(0);
            }
            yield break;
         }
         var columnNames = new string[reader.FieldCount];
         var columnTypes = new Type[reader.FieldCount];
         for (var i = 0; i < reader.FieldCount; i++)
         {
            columnNames[i] = reader.GetName(i);
            columnTypes[i] = EnsureNullable(reader.GetFieldType(i));
         }

         if (columnNames.Any(string.IsNullOrWhiteSpace) || columnNames.Length != columnNames.Distinct().Count())
         {
            // return AdoRow if any column  has no name, or if column names exist with same names
            var meta = new AdoRow.Meta { ColumnNames = columnNames, Index = new Dictionary<string, int>(), Types = columnTypes };
            var count = reader.FieldCount;
            while (reader.Read())
            {
               var values = new object[count];
               reader.GetValues(values);
               yield return new AdoRow(meta, values);
            }
            yield break;

         }
         var dtoType = AnonymousTypeHelper.GetOrCreateType(columnNames, columnTypes);
         var m = _readAll.MakeGenericMethod(dtoType);
         foreach (var x in (IEnumerable<dynamic>)m.Invoke(this, new object[] { reader, false, false }))
         {
            yield return x;
         }
      }

      public override IEnumerable<T> ReadAll<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
      {
         if (typeof(T) == typeof(IDictionary<string, object>) || typeof(T) == typeof(AdoRow))
         {
            foreach (var e in base.ReadAll(reader)) yield return (T)e;
            yield break;
         }

         foreach (var e in base.ReadAll<T>(reader, allowUnbindableFetchResults, allowUnbindableMembers))
         {
            yield return e;
         }
      }

      private static Type EnsureNullable(Type type)
      {
         return type.IsValueType && Nullable.GetUnderlyingType(type) == null
             ? typeof(Nullable<>).MakeGenericType(type)
             : type;

      }

   }

   public static partial class Extensions
   {
      public static IAdoContextInitializer EnableEmittedDynamicTypes(this IAdoContextInitializer self)
      {
         return self.BindSingleton<IAdoDataReaderManager, AdoDataReaderManagerEmittedDynamicTypesImpl>();
      }
   }
}