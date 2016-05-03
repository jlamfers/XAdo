using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoMultiBinderFactory
    {
        IList<IAdoReaderToMemberBinder<T>> InitializeMemberBinders<T,TNext>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, ref int nextIndex);
    }
}