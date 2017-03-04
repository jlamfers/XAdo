using System;
using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Interface
{
    public partial interface IXAdoDataReaderManager
    {
        IEnumerable<object> ReadAll(IDataReader reader);
        IEnumerable<T> ReadAll<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);

        IEnumerable<TResult> ReadAll<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDataReader reader,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, bool allowUnbindableFetchResults,
            bool allowUnbindableMembers);
    }

}
