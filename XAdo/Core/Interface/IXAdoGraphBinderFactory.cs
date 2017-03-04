using System;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IXAdoGraphBinderFactory
    {
        Func<IDataReader,T> CreateRecordBinder<T,TNext>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, ref int nextIndex);
    }
}