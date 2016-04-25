using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public partial class AdoDataReaderManagerEmittedDynamicTypesImpl
    {

        private static readonly MethodInfo _readAllAsync =
            typeof(IAdoDataReaderManager).GetMethods().Single(m => m.IsGenericMethod && m.GetGenericArguments().Length == 1 && m.Name == "ReadAllAsync").GetGenericMethodDefinition();

        public override async Task<List<dynamic>> ReadAllAsync(IDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            var columnNames = new string[reader.FieldCount];
            var columnTypes = new Type[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
                columnTypes[i] = EnsureNullable(reader.GetFieldType(i));
            }
            var dtoType = AnonymousTypeHelper.GetOrCreateType(columnNames, columnTypes);
            var m = _readAllAsync.MakeGenericMethod(dtoType);
            var taskHelper = ((ITaskHelper)Activator.CreateInstance(typeof(TaskHelper<>).MakeGenericType(dtoType))).SetMethod(m);
            var result = await taskHelper.InvokeAsync(this, reader);
            return (List<dynamic>) result;
        }

        public override async Task<List<T>> ReadAllAsync<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableProperties)
        {
            if (typeof(T) == typeof(IDictionary<string, object>) || typeof(T) == typeof(AdoRow))
            {
                var result = await base.ReadAllAsync(reader);
                return result.Cast<T>().ToList();
            }
            return await base.ReadAllAsync<T>(reader, allowUnbindableFetchResults, allowUnbindableProperties);
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