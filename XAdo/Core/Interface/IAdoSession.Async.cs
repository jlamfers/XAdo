using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
    public partial interface IAdoSession
    {
        Task<List<T>> QueryAsync<T>(string sql, object param = null, CommandType? commandType = null);
        Task<List<T>> QueryAsync<T>(string sql, Func<IDataRecord, T> factory, object param = null, CommandType? commandType = null);

        Task<T> ExecuteScalarAsync<T>(string sql, object param = null, CommandType? commandType = null);
        Task<object> ExecuteScalarAsync(string sql, object param = null, CommandType? commandType = null);
        Task<List<object>> QueryAsync(string sql, object param = null, CommandType? commandType = null);

        Task<AdoMultiResultReaderAsync> QueryMultipleAsync(string sql, object param = null,CommandType? commandType = null);
        Task<AdoMultiResultReaderAsync> QueryMultipleAsync(string sql, IEnumerable<Delegate> factories,  object param = null, CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, TResult>(string sql, Func<T1, T2, TResult> factory, object param = null,
            CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, T3, TResult>(string sql, Func<T1, T2, T3, TResult> factory,
            object param = null, CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(string sql, Func<T1, T2, T3, T4, TResult> factory,
            object param = null, CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, CommandType? commandType = null);

        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, CommandType? commandType = null);

        Task<int> ExecuteAsync(string sql, object param = null, CommandType? commandType = null);

        Task<bool> FlushSqlAsync();
    }
}
