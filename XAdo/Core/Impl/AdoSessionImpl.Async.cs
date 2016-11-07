using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace XAdo.Core.Impl
{
    public partial class AdoSessionImpl
    {

       public virtual async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.ExecuteScalarAsync<T>(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType);
        }
        public virtual async Task<object> ExecuteScalarAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.ExecuteScalarAsync(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType);
        }

       public virtual async Task<List<T>> QueryAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync<T>(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
       public virtual async Task<List<T>> QueryAsync<T>(string sql, Func<IDataRecord, T> factory, object param = null, CommandType? commandType = null)
       {
          EnsureNotDisposed();
          return await _connectionQueryManager.QueryAsync<T>(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType);
       }
       public virtual async Task<List<dynamic>> QueryAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType);
        }
        public virtual async Task<AdoMultiResultReaderAsync> QueryMultipleAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryMultipleAsync(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);

        }

        public virtual async Task<AdoMultiResultReaderAsync> QueryMultipleAsync(string sql, IEnumerable<Delegate> factories, object param = null, CommandType? commandType = null)
       {
          EnsureNotDisposed();
          return await _connectionQueryManager.QueryMultipleAsync(LazyInitializedConnection, sql, factories, param, _tr, _commandTimeout, commandType);
       }

       public virtual async Task<List<TResult>> QueryAsync<T1, T2, TResult>(string sql, Func<T1, T2, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, TResult>(string sql, Func<T1, T2, T3, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(string sql, Func<T1, T2, T3, T4, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(string sql, Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(string sql, Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string sql, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<int> ExecuteAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.ExecuteAsync(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType);
        }

    }
}
