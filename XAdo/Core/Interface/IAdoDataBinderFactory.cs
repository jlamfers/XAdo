using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace XAdo.Core.Interface
{
    public interface IAdoDataBinderFactory
    {
        IAdoPropertyBinder<TEntity> CreatePropertyBinder<TEntity>(PropertyInfo property, Type getterType, int index);
        Func<IDataReader, TResult> CreateScalarReader<TResult>(Type getterType);
        IList<IAdoPropertyBinder<T>> CreatePropertyBinders<T>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableProperties, int? firstColumnIndex = null, int? lastColumnIndex = null);
        bool IsBindableDataType(Type type);
    }
}