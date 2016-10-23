using System;
using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Interface
{
    public partial interface IAdoConnectionQueryManager
    {
        int Execute(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null);

        object ExecuteScalar(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null);

        T ExecuteScalar<T>(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null);

        IEnumerable<object> Query(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null);

        IEnumerable<T> Query<T>(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);

        IEnumerable<T> Query<T>(IDbConnection cn, string sql, Func<IDataRecord,T> factory,  object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null);

        AdoMultiResultReader QueryMultiple(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, TResult>(IDbConnection cn, string sql, Func<T1, T2, TResult> factory,
            object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, T3, TResult>(IDbConnection cn, string sql, Func<T1, T2, T3, TResult> factory,
            object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);

        IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false);
    }
}