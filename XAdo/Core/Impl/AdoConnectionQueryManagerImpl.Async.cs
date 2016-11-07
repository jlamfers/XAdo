using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace XAdo.Core.Impl
{
    public partial class AdoConnectionQueryManagerImpl
    {
        public virtual async Task<int> ExecuteAsync(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");
            var enumerable = param as IEnumerable;
            {
                if (enumerable != null && !(enumerable is IDictionary))
                {
                    return await ExecuteEnumerableParamAsync(cn, sql, enumerable, tr, commandTimeout, commandType);
                }
            }
            var wasopen = cn.State == ConnectionState.Open;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            try
            {
                using (var cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
                {
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (!wasopen)
                {
                    cn.Close();
                }
            }
        }

        protected virtual async Task<int> ExecuteEnumerableParamAsync(IDbConnection cn, string sql, IEnumerable param, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");
            if (param == null) throw new ArgumentNullException("param");

            var result = 0;
            var wasopen = cn.State == ConnectionState.Open;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            try
            {
                using (var cmd = (DbCommand)CreateCommand(cn, sql, null, tr, commandTimeout, commandType))
                {
                    var enumerator = param.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        FillParams(cmd, enumerator.Current);
                        result += await cmd.ExecuteNonQueryAsync();
                        cmd.CommandText = sql;
                    }
                    return result;
                }
            }
            finally
            {
                if (!wasopen)
                {
                    cn.Close();
                }
            }
        }

        public virtual async Task<object> ExecuteScalarAsync(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await ExecuteScalarAsync<object>(cn, sql, param, tr, commandTimeout, commandType);
        }

        public virtual async Task<T> ExecuteScalarAsync<T>(IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");
            var wasopen = cn.State == ConnectionState.Open;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            try
            {
                using (var cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
                {
                    var value = await cmd.ExecuteScalarAsync();
                    return value.CastTo<T>(_typeConverterFactory);
                }
            }
            finally
            {
                if (!wasopen)
                {
                    cn.Close();
                }
            }
        }

        public virtual async Task<List<dynamic>> QueryAsync(IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");
            var wasopen = cn.State == ConnectionState.Open;
            var skipClose = false;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            try
            {
                List<dynamic> result;
                using (var cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        result = await _dataReaderManager.ReadAllAsync(reader);
                    }
                }
                if (!wasopen)
                {
                    skipClose = true;
                    cn.Close();
                }
                return result;
            }
            finally
            {
                if (!wasopen && !skipClose)
                {
                    cn.Close();
                }
            }
        }

        public virtual async Task<List<T>> QueryAsync<T>(IDbConnection cn, string sql, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");
            var wasopen = cn.State == ConnectionState.Open;
            var skipClose = false;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            try
            {
                List<T> result;
                using (var cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        result =
                            await
                                _dataReaderManager.ReadAllAsync<T>(reader, allowUnbindableFetchResults,
                                    allowUnbindableMembers);
                    }
                }
                if (!wasopen)
                {
                    skipClose = true;
                    cn.Close();
                }
                return result;
            }
            finally
            {
                if (!wasopen && !skipClose)
                {
                    cn.Close();
                }
            }
        }

       public virtual async Task<List<T>> QueryAsync<T>(IDbConnection cn, string sql, Func<IDataRecord, T> factory, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null)
       {
          if (cn == null) throw new ArgumentNullException("cn");
          if (sql == null) throw new ArgumentNullException("sql");
          var wasopen = cn.State == ConnectionState.Open;
          var skipClose = false;
          if (!wasopen)
          {
             await EnsureOpenAsync(cn);
          }
          try
          {
             var result = new List<T>();
             using (var cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
             {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                   while (await reader.NextResultAsync())
                      result.Add(factory(reader));
                }
             }
             if (!wasopen)
             {
                skipClose = true;
                cn.Close();
             }
             return result;
          }
          finally
          {
             if (!wasopen && !skipClose)
             {
                cn.Close();
             }
          }
       }

       public virtual async Task<AdoMultiResultReaderAsync> QueryMultipleAsync(IDbConnection cn, string sql,
            object param = null, IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");

            var wasopen = cn.State == ConnectionState.Open;
            var skipClose = false;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            DbCommand cmd = null;
            DbDataReader reader = null;
            try
            {
                cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType);
                reader =
                    await cmd.ExecuteReaderAsync(wasopen ? CommandBehavior.Default : CommandBehavior.CloseConnection);
                var multiReader = new AdoMultiResultReaderAsync(reader, cmd, allowUnbindableFetchResults,
                    allowUnbindableMembers, _dataReaderManager);
                skipClose = true;
                return multiReader;
            }
            catch
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try
                        {
                            cmd.Cancel();
                        }
                        catch
                        {
                        }
                    }
                    reader.Dispose();
                }
                if (cmd != null) cmd.Dispose();
                if (!wasopen && !skipClose) cn.Close();
                throw;
            }
        }

       public virtual async Task<AdoMultiResultReaderAsync> QueryMultipleAsync(IDbConnection cn, string sql, IEnumerable<Delegate> factories, object param = null,IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
       {
          if (cn == null) throw new ArgumentNullException("cn");
          if (sql == null) throw new ArgumentNullException("sql");

          var wasopen = cn.State == ConnectionState.Open;
          var skipClose = false;
          if (!wasopen)
          {
             await EnsureOpenAsync(cn);
          }
          DbCommand cmd = null;
          DbDataReader reader = null;
          try
          {
             cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType);
             reader =
                 await cmd.ExecuteReaderAsync(wasopen ? CommandBehavior.Default : CommandBehavior.CloseConnection);
             var multiReader = new AdoMultiResultReaderAsync(reader, cmd, factories, _dataReaderManager);
             skipClose = true;
             return multiReader;
          }
          catch
          {
             if (reader != null)
             {
                if (!reader.IsClosed)
                {
                   try
                   {
                      cmd.Cancel();
                   }
                   catch
                   {
                   }
                }
                reader.Dispose();
             }
             if (cmd != null) cmd.Dispose();
             if (!wasopen && !skipClose) cn.Close();
             throw;
          }
       }

       public virtual async Task<List<TResult>> QueryAsync<T1, T2, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null,
            CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return await QueryAsync<T1, T2, TVoid, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(cn, sql,
                (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2), param, tr, commandTimeout,
                commandType, allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null,
            CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return await QueryAsync<T1, T2, T3, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(cn, sql,
                (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3), param, tr, commandTimeout,
                commandType, allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return await QueryAsync<T1, T2, T3, T4, TVoid, TVoid, TVoid, TVoid, TResult>(cn, sql,
                (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4), param, tr, commandTimeout,
                commandType, allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(IDbConnection cn, string sql,
            Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return await QueryAsync<T1, T2, T3, T4, T5, TVoid, TVoid, TVoid, TResult>(cn, sql,
                (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4, t5), param, tr, commandTimeout,
                commandType, allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return await QueryAsync<T1, T2, T3, T4, T5, T6, TVoid, TVoid, TResult>(cn, sql,
                (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4, t5, t6), param, tr, commandTimeout,
                commandType, allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, IDbTransaction tr = null,
            int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return await QueryAsync<T1, T2, T3, T4, T5, T6, T7, TVoid, TResult>(cn, sql,
                (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4, t5, t6, t7), param, tr, commandTimeout,
                commandType, allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual async Task<List<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDbConnection cn,
            string sql, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null,
            IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
            bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");
            if (factory == null) throw new ArgumentNullException("factory");

            var wasopen = cn.State == ConnectionState.Open;
            var skipClose = false;
            if (!wasopen)
            {
                await EnsureOpenAsync(cn);
            }
            try
            {
                List<TResult> result;
                using (var cmd = (DbCommand)CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        result =
                            await
                                _dataReaderManager.ReadAllAsync(reader, factory, allowUnbindableFetchResults,
                                    allowUnbindableMembers);
                    }
                }
                if (!wasopen)
                {
                    skipClose = true;
                    cn.Close();
                }
                return result;
            }
            finally
            {
                if (!wasopen && !skipClose)
                {
                    cn.Close();
                }
            }
        }

        private static async Task EnsureOpenAsync(IDbConnection cn)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            var dbc = (DbConnection)cn;
            dbc.Close();
            await dbc.OpenAsync();
        }

    }
}
