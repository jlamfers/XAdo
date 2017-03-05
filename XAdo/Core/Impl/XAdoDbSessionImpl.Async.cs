using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace XAdo.Core.Impl
{
    public partial class XAdoDbSessionImpl
    {

       public virtual async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.ExecuteScalarAsync<T>(LazyConnection.Value, sql, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType);
        }
        public virtual async Task<object> ExecuteScalarAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.ExecuteScalarAsync(LazyConnection.Value, sql, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType);
        }

       public virtual async Task<List<T>> QueryAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync<T>(LazyConnection.Value, sql, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
       public virtual async Task<List<T>> QueryAsync<T>(string sql, Func<IDataRecord, T> factory, object param = null, CommandType? commandType = null)
       {
          EnsureNotDisposed();
          return await _connectionQueryManager.QueryAsync<T>(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType);
       }
       public virtual async Task<List<dynamic>> QueryAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType);
        }
        public virtual async Task<XAdoMultiResultReaderAsync> QueryMultipleAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryMultipleAsync(LazyConnection.Value, sql, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);

        }

        public virtual async Task<XAdoMultiResultReaderAsync> QueryMultipleAsync(string sql, IEnumerable<Delegate> factories, object param = null, CommandType? commandType = null)
       {
          EnsureNotDisposed();
          return await _connectionQueryManager.QueryMultipleAsync(LazyConnection.Value, sql, factories, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType);
       }

       public virtual async Task<List<TResult>> QueryAsync<T1, T2, TResult>(string sql, Func<T1, T2, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, TResult>(string sql, Func<T1, T2, T3, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(string sql, Func<T1, T2, T3, T4, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(string sql, Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(string sql, Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string sql, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.QueryAsync(LazyConnection.Value, sql, factory, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
        }
        public virtual async Task<int> ExecuteAsync(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return await _connectionQueryManager.ExecuteAsync(LazyConnection.Value, sql, param, _tr == null ? null : _tr.Value, _commandTimeout, commandType);
        }

        public virtual async Task<bool> FlushSqlBatchAsync()
       {
          EnsureNotDisposed();
          if (_sqlBatch != null && _sqlBatch.Count > 0)
          {
             await _sqlBatch.FlushAsync(this);
             return true;
          }
          return false;
       }
    }
}
