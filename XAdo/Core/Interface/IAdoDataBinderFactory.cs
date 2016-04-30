using System;
using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoDataBinderFactory
    {
        Func<IDataReader, TResult> CreateScalarReader<TResult>(Type getterType);
        IList<IAdoMemberBinder<TEntity>> CreateMemberBinders<TEntity>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableProperties, int? firstColumnIndex = null, int? lastColumnIndex = null);
        bool IsBindableDataType(Type type);
    }
}