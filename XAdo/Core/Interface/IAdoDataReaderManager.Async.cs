using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
    public partial interface IAdoDataReaderManager
    {


        Task<List<object>> ReadAllAsync(IDataReader reader);

        Task<List<T>> ReadAllAsync<T>(IDataReader reader, bool allowUnbindableFetchResults,
            bool allowUnbindableMembers);

        Task<List<TResult>> ReadAllAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDataReader r,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, bool allowUnbindableFetchResults,
            bool allowUnbindableMembers);
    }
}
