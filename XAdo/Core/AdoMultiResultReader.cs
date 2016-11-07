using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core
{
    public class AdoMultiResultReader : IDisposable
    {
        private int _currentResultIndex;
        private IDataReader _reader;
        private IDbCommand _command;
       private readonly Delegate[] _factories;
       private readonly bool _allowUnbindableFetchResults;
        private readonly bool _allowUnbindableMembers;
        private readonly IAdoDataReaderManager _dataReaderQuery;

        internal AdoMultiResultReader(IDataReader reader, IDbCommand command, bool allowUnbindableFetchResults, bool allowUnbindableMembers, IAdoDataReaderManager dataReaderQuery)
        {
            _reader = reader;
            _command = command;
            _allowUnbindableFetchResults = allowUnbindableFetchResults;
            _allowUnbindableMembers = allowUnbindableMembers;
            _dataReaderQuery = dataReaderQuery;
        }

        internal AdoMultiResultReader(IDataReader reader, IDbCommand command, IEnumerable<Delegate> factories, IAdoDataReaderManager dataReaderQuery)
        {
           _reader = reader;
           _command = command;
           _factories = factories.ToArray();
           _allowUnbindableFetchResults = true;
           _allowUnbindableMembers = true;
           _dataReaderQuery = dataReaderQuery;
        }

        public IEnumerable<dynamic> Read(bool buffered = true)
        {
            EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public IEnumerable<T> Read<T>(bool buffered = true) 
        {
            EnsureNotDisposed();
            var enumerable = CurrentResultReader<T>(_currentResultIndex);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1,T2,TResult>(Func<T1, T2, TResult> f, bool buffered = true)
        {
            EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> f, bool buffered = true)
        {
           EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1, T2, T3,T4, TResult>(Func<T1, T2, T3,T4, TResult> f, bool buffered = true)
        {
           EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1, T2, T3, T4,T5, TResult>(Func<T1, T2, T3, T4,T5, TResult> f, bool buffered = true)
        {
           EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5,T6, TResult> f, bool buffered = true)
        {
           EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1, T2, T3, T4, T5, T6,T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7,TResult> f, bool buffered = true)
        {
           EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }
        public IEnumerable<TResult> Read<T1, T2, T3, T4, T5, T6, T7,T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7,T8, TResult> f, bool buffered = true)
        {
           EnsureNotDisposed();
            var enumerable = CurrentResultReader(_currentResultIndex, f);
            return buffered ? enumerable.ToList() : enumerable;
        }

        private IEnumerable<T> CurrentResultReader<T>(int index)
        {
            if (index != _currentResultIndex) yield break;
            try
            {
               if (_factories != null)
               {
                  var f = (Func<IDataRecord,T>)_factories[index];
                  while (_reader.Read())
                  {
                     yield return f(_reader);
                  }
               }
               else
               {

                  foreach (
                     var e in
                        _dataReaderQuery.ReadAll<T>(_reader, _allowUnbindableFetchResults, _allowUnbindableMembers))
                     yield return e;
               }
            }
            finally
            {
                if (index == _currentResultIndex)
                {
                    NextResult();
                }
            }
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, TResult>(int index, Func<T1, T2, TResult> f)
        {
            return CurrentResultReader<T1, T2, TVoid, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(index, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2));
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, T3, TResult>(int index, Func<T1, T2, T3, TResult> f)
        {
            return CurrentResultReader<T1, T2, T3, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(index, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3));
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, T3, T4, TResult>(int index, Func<T1, T2, T3, T4, TResult> f)
        {
            return CurrentResultReader<T1, T2, T3, T4, TVoid, TVoid, TVoid, TVoid, TResult>(index, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4));
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, T3, T4, T5, TResult>(int index, Func<T1, T2, T3, T4, T5, TResult> f)
        {
            return CurrentResultReader<T1, T2, T3, T4, T5, TVoid, TVoid, TVoid, TResult>(index, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5));
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, T3, T4, T5, T6, TResult>(int index, Func<T1, T2, T3, T4, T5, T6, TResult> f)
        {
            return CurrentResultReader<T1, T2, T3, T4, T5, T6,TVoid, TVoid, TResult>(index, (t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6));
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, T3, T4, T5, T6, T7, TResult>(int index, Func<T1, T2, T3, T4, T5, T6, T7, TResult> f)
        {
            return CurrentResultReader<T1, T2, T3, T4, T5, T6, T7, TVoid,TResult>(index,(t1, t2, t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6, t7));
        }
        private IEnumerable<TResult> CurrentResultReader<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(int index, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> f)
        {
           EnsureNoFactories();
            if (index != _currentResultIndex) yield break;
            try
            {
                foreach (var e in _dataReaderQuery.ReadAll(_reader, f, _allowUnbindableFetchResults, _allowUnbindableMembers)) yield return e;
                if (index != _currentResultIndex) yield break;
                index = -1;
                NextResult();
            }
            finally
            {
                if (index == _currentResultIndex)
                {
                    NextResult();
                }
            }
        }

        private IEnumerable<dynamic> CurrentResultReader(int index)
        {
            if (index != _currentResultIndex) yield break;
            try
            {
               if (_factories != null)
               {
                  var f = _factories[index];
                  while (_reader.Read())
                  {
                     yield return f.DynamicInvoke(_reader);
                  }
               }
               else
               {
                  foreach (var e in _dataReaderQuery.ReadAll(_reader)) yield return e;
               }
               if (index != _currentResultIndex) yield break;
               index = -1;
               NextResult();
            }
            finally
            {
                if (index == _currentResultIndex)
                {
                    NextResult();
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(GetType().Name, "You possibly have consumed all datareaders while attempting to read another one.");
            }
        }
        private void EnsureNoFactories()
        {
           if (_factories != null)
           {
              throw new InvalidOperationException("You cannot bind a graph while using custom binding factories. Invoke read using a single generic argument.");
           }
        }

        private void NextResult()
        {
            if (_reader.NextResult())
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
