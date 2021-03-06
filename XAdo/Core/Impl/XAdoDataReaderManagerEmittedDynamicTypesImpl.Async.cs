using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public partial class XAdoDataReaderManagerEmittedDynamicTypesImpl
    {

        private static readonly MethodInfo _readAllAsync =
            typeof(IXAdoDataReaderManager).GetMethods().Single(m => m.IsGenericMethod && m.GetGenericArguments().Length == 1 && m.Name == "ReadAllAsync").GetGenericMethodDefinition();

        public override async Task<List<dynamic>> ReadAllAsync(IDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
           if (reader.FieldCount == 1)
           {
              var list = new List<dynamic>();
              var dbreader = (DbDataReader)reader;
              while (await dbreader.ReadAsync())
              {
                 list.Add(dbreader.IsDBNull(0) ? null : dbreader.GetValue(0));
              }
              return list;
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
               // return AdoRow if any column  has no name
               var meta = new XAdoRow.Meta { ColumnNames = columnNames, Index = new Dictionary<string, int>(), Types = columnTypes };
               var count = reader.FieldCount;
               var dbreader = (DbDataReader)reader;
               var result2 = new List<dynamic>();
               while (await dbreader.ReadAsync())
               {
                  var values = new object[count];
                  reader.GetValues(values);
                  result2.Add(new XAdoRow(meta, values));
               }
               return result2;
            }

            var dtoType = AnonymousTypeHelper.GetOrCreateType(columnNames, columnTypes);
            var m = _readAllAsync.MakeGenericMethod(dtoType);
            var taskHelper = ((ITaskHelper)Activator.CreateInstance(typeof(TaskHelper<>).MakeGenericType(dtoType))).SetMethod(m);
            var result = await taskHelper.InvokeAsync(this, reader);
            return (List<dynamic>) result;
        }

        public override async Task<List<T>> ReadAllAsync<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
        {
            if (typeof(T) == typeof(IDictionary<string, object>) || typeof(T) == typeof(XAdoRow))
            {
                var result = await base.ReadAllAsync(reader);
                return result.Cast<T>().ToList();
            }
            return await base.ReadAllAsync<T>(reader, allowUnbindableFetchResults, allowUnbindableMembers);
        }


        private interface ITaskHelper
        {
            ITaskHelper SetMethod(MethodInfo mi);
            Task<object> InvokeAsync(object target,IDataReader reader);
        }

        private class TaskHelper<T> : ITaskHelper
        {
            private MethodInfo _mi;

            public ITaskHelper SetMethod(MethodInfo mi)
            {
                _mi = mi;
                return this;
            }

            public async Task<object> InvokeAsync(object target, IDataReader reader)
            {
                var result = await (Task<List<T>>)_mi.Invoke(target, new object[] { reader, false, false });
                return result.Cast<dynamic>().ToList();
            }
        }

    }
}