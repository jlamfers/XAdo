using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XAdo.Core;

namespace XAdo
{
    public static partial class DbConnectionExtensions
    {

        public static async Task<int> ExecuteAsync(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await QueryManager.ExecuteAsync(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static async Task<object> ExecuteScalarAsync(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await QueryManager.ExecuteScalarAsync(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static async Task<T> ExecuteScalarAsync<T>(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await QueryManager.ExecuteScalarAsync<T>(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static async Task<List<dynamic>> QueryAsync(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await QueryManager.QueryAsync(cn, sql, param, tr, commandTimeout, commandType);
        }

        public static async Task<List<T>> QueryAsync<T>(this IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync<T>(cn, sql, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<AdoMultiResultReaderAsync> QueryMultipleAsync(this IDbConnection cn, string sql,
            object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryMultipleAsync(cn, sql, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null,
            CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, T3, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null,
            CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(this IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(this IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(this IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(this IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }

        public static async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
            this IDbConnection cn, string sql, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory,
            object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            return
                await
                    QueryManager.QueryAsync(cn, sql, factory, param, tr, commandTimeout, commandType,
                        allowUnbindableFetchResults, allowUnbindableProperties);
        }
    }
}