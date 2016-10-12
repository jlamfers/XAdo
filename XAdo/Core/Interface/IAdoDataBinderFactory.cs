using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace XAdo.Core.Interface
{
    public interface IAdoDataBinderFactory
    {
       Func<IDataReader, TResult> CreateRecordBinder<TResult>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null);
       Func<IDataReader, TResult> CreateScalarReader<TResult>(Type getterType);
        //Func<IDataReader, TResult> TryCreateCtorBinder<TResult>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null);
        //IList<IAdoReaderToMemberBinder<TEntity>> CreateMemberBinders<TEntity>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null);
        //bool IsBindableDataType(Type type);
        IEnumerable<MemberInfo> GetBindableMembers(Type type, bool canWrite=true);
    }
}