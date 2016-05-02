using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace XAdo.Core.Interface
{
    public interface IAdoDataBinderFactory
    {
        Func<IDataReader, TResult> CreateScalarReader<TResult>(Type getterType);
        IList<IAdoMemberBinder<TEntity>> CreateMemberBinders<TEntity>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null);
        bool IsBindableDataType(Type type);
        IEnumerable<MemberInfo> GetBindableMembers(Type type);
    }
}