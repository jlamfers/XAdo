using System;
using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Interface
{
    public partial interface IAdoSession : IDisposable
    {
        T ExecuteScalar<T>(string sql, object param = null, CommandType? commandType = null);
        object ExecuteScalar(string sql, object param = null, CommandType? commandType = null);
        IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, CommandType? commandType = null);
        IEnumerable<dynamic> Query(string sql, object param = null, bool buffered = true, CommandType? commandType = null);
        AdoMultiResultReader QueryMultiple(string sql, object param = null, CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, TResult>(string sql, Func<T1, T2, TResult> factory, object param = null,
            bool buffered = true, CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, T3, TResult>(string sql, Func<T1, T2, T3, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(string sql, Func<T1, T2, T3, T4, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(string sql, Func<T1, T2, T3, T4, T5, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null);

        int Execute(string sql, object param = null, CommandType? commandType = null);
        bool HasTransaction { get; }
        IAdoSession BeginTransaction(bool autoCommit = false);
        bool Commit();
        bool Rollback();
    }
}