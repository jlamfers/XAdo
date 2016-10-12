using System;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoMultiBinderFactory
    {
        Func<IDataReader,T> InitializeMemberBinders<T,TNext>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, ref int nextIndex);
    }
}