using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace XAdo.Core.Interface
{
    public interface IXAdoDataBinderFactory
    {
       Func<IDataReader, TEntity> CreateRecordBinder<TEntity>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null);
       Func<IDataReader, TValue> CreateScalarBinder<TValue>(Type getterType);
       IEnumerable<MemberInfo> GetBindableMembers(Type type, bool canWrite=true);
    }
}