using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
    public partial interface IAdoConnectionQueryManager
    {
        Task<int> ExecuteAsync(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<object> ExecuteScalarAsync(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<List<object>> QueryAsync(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<List<T>> QueryAsync<T>(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<AdoMultiResultReaderAsync> QueryMultipleAsync(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, TResult>(IDbConnection cn, string sql, Func<T1, T2, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, T3, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, T4, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
    }
}
