using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoMultiBinderFactory
    {
        IList<IAdoMemberBinder<T>> InitializePropertyBinders<T,TNext>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableProperties, ref int nextIndex);
    }
}