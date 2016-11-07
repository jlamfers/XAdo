using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using XAdo.Core.Interface;

namespace XAdo.Core
{
    public class AdoMultiResultReaderAsync
    {
        private int _currentResultIndex;
        private DbDataReader _reader;
        private IDbCommand _command;
        private readonly bool _allowUnbindableFetchResults;
        private readonly bool _allowUnbindableMembers;
        private readonly IAdoDataReaderManager _dataReaderQuery;
       private Delegate[] _factories;

       internal AdoMultiResultReaderAsync(IDataReader reader, IDbCommand command, bool allowUnbindableFetchResults, bool allowUnbindableMembers, IAdoDataReaderManager dataReaderQuery)
        {
            _reader = (DbDataReader)reader;
            _command = command;
            _allowUnbindableFetchResults = allowUnbindableFetchResults;
            _allowUnbindableMembers = allowUnbindableMembers;
            _dataReaderQuery = dataReaderQuery;
        }

       public AdoMultiResultReaderAsync(IDataReader reader, DbCommand command, IEnumerable<Delegate> factories, IAdoDataReaderManager dataReaderQuery)
        {
           _reader = (DbDataReader)reader;
           _command = command;
           _factories = factories.ToArray();
           _allowUnbindableFetchResults = true;
           _allowUnbindableMembers = true;
           _dataReaderQuery = dataReaderQuery;
        }

        public async Task<List<dynamic>>  ReadAsync()
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync(_currentResultIndex);
        }

        public async Task<List<T>> ReadAsync<T>() 
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T>(_currentResultIndex);
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, TResult>(Func<T1, T2, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T1, T2, TVoid, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(_currentResultIndex, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2));
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T1, T2, T3, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(_currentResultIndex, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3));
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T1, T2, T3, T4, TVoid, TVoid, TVoid, TVoid, TResult>(_currentResultIndex, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4));
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T1, T2, T3, T4, T5, TVoid, TVoid, TVoid, TResult>(_currentResultIndex, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5));
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T1, T2, T3, T4, T5, T6, TVoid, TVoid, TResult>(_currentResultIndex, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6));
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync<T1, T2, T3, T4, T5, T6, T7, TVoid, TResult>(_currentResultIndex, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6, t7));
        }
        public async Task<List<TResult>> ReadAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> f)
        {
            EnsureNotDisposed();
            return await CurrentResultReaderAsync(_currentResultIndex, f);
        }

        private async Task<List<T>> CurrentResultReaderAsync<T>(int index)
        {
            if (index != _currentResultIndex)
            {
                return new List<T>();
            }
           List<T> result;
           if (_factories == null)
           {
              result =
                 await _dataReaderQuery.ReadAllAsync<T>(_reader, _allowUnbindableFetchResults, _allowUnbindableMembers);
           }
           else
           {
              result = new List<T>();
              var f = (Func<IDataRecord,T>)_factories[index];
              while (await _reader.ReadAsync())
              {
                 result.Add(f(_reader));
              }
           }

           if (index == _currentResultIndex)
            {
                await NextResultAsync();
            }
            return result;
        }
        private async Task<List<TResult>> CurrentResultReaderAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(int index, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> f)
        {
           EnsureNoFactories();
            if (index != _currentResultIndex)
            {
                return new List<TResult>();
            }
            var result = await _dataReaderQuery.ReadAllAsync(_reader, f, _allowUnbindableFetchResults, _allowUnbindableMembers);
            if (index == _currentResultIndex)
            {
                await NextResultAsync();
            }
            return result;
        }
        private async Task<List<dynamic>> CurrentResultReaderAsync(int index)
        {
            if (index != _currentResultIndex)
            {
                return new List<dynamic>();
            }
           List<object> result;
           if (_factories == null)
           {
              result = await _dataReaderQuery.ReadAllAsync(_reader);
           }
           else
           {
              result = new List<object>();
              var f = _factories[index];
              while (await _reader.ReadAsync())
              {
                 result.Add(f.DynamicInvoke(_reader));
              }
           }
           if (index == _currentResultIndex)
            {
                await NextResultAsync();
            }
            return result;
        }

        private void EnsureNoFactories()
        {
           if (_factories != null)
           {
              throw new InvalidOperationException("You cannot bind a graph while using custom binding factories. Invoke read using a single generic argument.");
           }
        }


        private void EnsureNotDisposed()
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private async Task NextResultAsync()
        {
            if (await _reader.NextResultAsync())
            {
                _currentResultIndex++;
            }
            else
            {
                _reader.Dispose();
                _reader = null;
                Dispose();
            }

        }

        public void Dispose()
        {
            if (_reader != null)
            {
                if (!_reader.IsClosed && _command != null) _command.Cancel();
                _reader.Dispose();
                _reader = null;
            }
            if (_command != null)
            {
                _command.Dispose();
                _command = null;
            }
        }

    }
}
