using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core;
using XAdo.Core.Interface;

namespace XAdo
{
    /// <summary>
    /// This class extends IDbConnection like Dapper
    /// </summary>
    public static partial class DbConnectionExtensions
    {

        private static IAdoConnectionQueryManager QueryManager
        {
            get { return AdoContext.Default.GetInstance<IAdoConnectionQueryManager>();}
        }

        public static int Execute(this IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryManager.Execute(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static object ExecuteScalar(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryManager.ExecuteScalar(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static T ExecuteScalar<T>(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryManager.ExecuteScalar<T>(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static IEnumerable<dynamic> Query(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var enumerable = QueryManager.Query(cn, sql, param, tr, commandTimeout, commandType);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query<T>(cn, sql, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static AdoMultiResultReader QueryMultiple(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            return QueryManager.QueryMultiple(cn, sql, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static IEnumerable<TResult> Query<T1, T2, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, TResult> factory, object param = null, IDbTransaction tr = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, TResult> factory, object param = null, IDbTransaction tr = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, TResult> factory, object param = null, IDbTransaction tr = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, IDbTransaction tr = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, IDbTransaction tr = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, IDbTransaction tr = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null,
            IDbTransaction tr = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            var enumerable = QueryManager.Query(cn, sql, factory, param, tr, commandTimeout, commandType,
                allowUnbindableFetchResults, allowUnbindableProperties);
            return buffered ? enumerable.ToList() : enumerable;
        }

    }
}