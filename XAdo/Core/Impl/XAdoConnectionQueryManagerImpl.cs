using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{


   public partial class XAdoConnectionQueryManagerImpl : IXAdoConnectionQueryManager
   {
      private readonly IXAdoDataReaderManager _dataReaderManager;
      private readonly IXAdoCommandFactory _commandFactory;
      private readonly IXAdoTypeConverterFactory _typeConverterFactory;
      private readonly IAdoSqlInterceptor _sqlInterceptor;

      public XAdoConnectionQueryManagerImpl(IXAdoDataReaderManager dataReaderManager, IXAdoCommandFactory commandFactory, IXAdoTypeConverterFactory typeConverterFactory, IAdoSqlInterceptor sqlInterceptor = null)
      {
         if (dataReaderManager == null) throw new ArgumentNullException("dataReaderManager");
         if (commandFactory == null) throw new ArgumentNullException("commandFactory");
         if (typeConverterFactory == null) throw new ArgumentNullException("typeConverterFactory");
         _dataReaderManager = dataReaderManager;
         _commandFactory = commandFactory;
         _typeConverterFactory = typeConverterFactory;
         _sqlInterceptor = sqlInterceptor;
      }

      public virtual int Execute(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Execute) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var enumerable = param as IEnumerable;
         {
            if (enumerable != null && !(enumerable is IDictionary))
            {
               return ExecuteEnumerableParam(cn, sql, enumerable, tr, commandTimeout, commandType);
            }
         }
         var wasopen = cn.State == ConnectionState.Open;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               return cmd.ExecuteNonQuery();
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

      protected virtual int ExecuteEnumerableParam(IDbConnection cn, string sql, IEnumerable param,
          IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");
         if (param == null) throw new ArgumentNullException("param");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.EnumerableParam) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = (IEnumerable)interception.Arguments;
            commandType = interception.CommandType;
         }


         var result = 0;
         var wasopen = cn.State == ConnectionState.Open;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, null, tr, commandTimeout, commandType))
            {
               var enumerator = param.GetEnumerator();
               while (enumerator.MoveNext())
               {
                  FillParams(cmd, enumerator.Current);
                  result += cmd.ExecuteNonQuery();
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

      public virtual object ExecuteScalar(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null)
      {
         return ExecuteScalar<object>(cn, sql, param, tr, commandTimeout, commandType);
      }

      public virtual T ExecuteScalar<T>(IDbConnection cn, string sql, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Scalar, typeof(T)) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }

         var wasopen = cn.State == ConnectionState.Open;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               return cmd.ExecuteScalar().CastTo<T>(_typeConverterFactory);
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


      public virtual IEnumerable<dynamic> Query(IDbConnection cn, string sql, object param = null,
          IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Query) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               using (var reader = cmd.ExecuteReader())
               {
                  foreach (var e in _dataReaderManager.ReadAll(reader)) yield return e;
               }
            }
            if (wasopen) yield break;
            skipClose = true;
            cn.Close();
         }
         finally
         {
            if (!wasopen && !skipClose)
            {
               cn.Close();
            }
         }
      }

      public virtual IEnumerable<T> Query<T>(IDbConnection cn, string sql, object param = null,
          IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
          bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Query, typeof(T)) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               using (var reader = cmd.ExecuteReader())
               {
                  foreach (
                      var e in
                          _dataReaderManager.ReadAll<T>(reader, allowUnbindableFetchResults,
                              allowUnbindableMembers)) yield return e;
               }
            }
            if (wasopen) yield break;
            skipClose = true;
            cn.Close();
         }
         finally
         {
            if (!wasopen && !skipClose)
            {
               cn.Close();
            }
         }
      }

      public virtual IEnumerable<T> Query<T>(IDbConnection cn, string sql, Func<IDataRecord, T> factory, object param = null, IDbTransaction tr = null,
         int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Query, typeof(T)) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               using (var reader = cmd.ExecuteReader())
               {
                  while (reader.Read())
                  {
                     yield return factory(reader);
                  }
               }
            }
            if (wasopen) yield break;
            skipClose = true;
            cn.Close();
         }
         finally
         {
            if (!wasopen && !skipClose)
            {
               cn.Close();
            }
         }
      }

      public virtual IEnumerable<T> Query<T>(IDbConnection cn, string sql, Func<IDataReader, T> factory, object param = null, IDbTransaction tr = null,
         int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Query, typeof(T)) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               using (var reader = cmd.ExecuteReader())
               {
                  while (reader.Read())
                     yield return factory(reader);
               }
            }
            if (wasopen) yield break;
            skipClose = true;
            cn.Close();
         }
         finally
         {
            if (!wasopen && !skipClose)
            {
               cn.Close();
            }
         }
      }


      public virtual XAdoMultiResultReader QueryMultiple(IDbConnection cn, string sql, object param = null,
          IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null,
          bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Multiple) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         IDbCommand cmd = null;
         IDataReader reader = null;
         try
         {
            cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType);
            reader = cmd.ExecuteReader(wasopen ? CommandBehavior.Default : CommandBehavior.CloseConnection);
            var multiReader = new XAdoMultiResultReader(reader, cmd, allowUnbindableFetchResults,allowUnbindableMembers, _dataReaderManager);
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

      public virtual XAdoMultiResultReader QueryMultiple(IDbConnection cn, string sql, IEnumerable<Delegate> factories, object param = null,
         IDbTransaction tr = null, int? commandTimeout = null, CommandType? commandType = null)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");
         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Multiple) { Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         IDbCommand cmd = null;
         IDataReader reader = null;
         try
         {
            cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType);
            reader = cmd.ExecuteReader(wasopen ? CommandBehavior.Default : CommandBehavior.CloseConnection);
            var multiReader = new XAdoMultiResultReader(reader, cmd, factories, _dataReaderManager);
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

      public virtual IEnumerable<TResult> Query<T1, T2, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null,
          CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (factory == null) throw new ArgumentNullException("factory");
         return Query<T1, T2, TVoid, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(cn, sql,
             (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2), param, tr, commandTimeout,
             commandType, allowUnbindableFetchResults, allowUnbindableMembers);
      }

      public virtual IEnumerable<TResult> Query<T1, T2, T3, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, T3, TResult> factory, object param = null, IDbTransaction tr = null, int? commandTimeout = null,
          CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (factory == null) throw new ArgumentNullException("factory");


         return Query<T1, T2, T3, TVoid, TVoid, TVoid, TVoid, TVoid, TResult>(cn, sql,
             (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3), param, tr, commandTimeout,
             commandType, allowUnbindableFetchResults, allowUnbindableMembers);
      }

      public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, T3, T4, TResult> factory, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (factory == null) throw new ArgumentNullException("factory");
         return Query<T1, T2, T3, T4, TVoid, TVoid, TVoid, TVoid, TResult>(cn, sql,
             (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4), param, tr, commandTimeout,
             commandType, allowUnbindableFetchResults, allowUnbindableMembers);
      }

      public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (factory == null) throw new ArgumentNullException("factory");
         return Query<T1, T2, T3, T4, T5, TVoid, TVoid, TVoid, TResult>(cn, sql,
             (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4, t5), param, tr, commandTimeout,
             commandType, allowUnbindableFetchResults, allowUnbindableMembers);
      }

      public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (factory == null) throw new ArgumentNullException("factory");
         return Query<T1, T2, T3, T4, T5, T6, TVoid, TVoid, TResult>(cn, sql,
             (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4, t5, t6), param, tr, commandTimeout,
             commandType, allowUnbindableFetchResults, allowUnbindableMembers);
      }

      public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (factory == null) throw new ArgumentNullException("factory");
         return Query<T1, T2, T3, T4, T5, T6, T7, TVoid, TResult>(cn, sql,
             (t1, t2, t3, t4, t5, t6, t7, t8) => factory(t1, t2, t3, t4, t5, t6, t7), param, tr, commandTimeout,
             commandType, allowUnbindableFetchResults, allowUnbindableMembers);
      }

      public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDbConnection cn, string sql,
          Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, IDbTransaction tr = null,
          int? commandTimeout = null, CommandType? commandType = null, bool allowUnbindableFetchResults = true,
          bool allowUnbindableMembers = false)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         if (sql == null) throw new ArgumentNullException("sql");
         if (factory == null) throw new ArgumentNullException("factory");

         if (_sqlInterceptor != null)
         {
            var interception = new AdoSqlInterception(SqlExecutionType.Query, typeof(TResult)) {Arguments = param, Sql = sql, CommandType = commandType };
            _sqlInterceptor.BeforeExecute(interception);
            sql = interception.Sql;
            param = interception.Arguments;
            commandType = interception.CommandType;
         }


         var wasopen = cn.State == ConnectionState.Open;
         var skipClose = false;
         if (!wasopen)
         {
            EnsureOpen(cn);
         }
         try
         {
            using (var cmd = CreateCommand(cn, sql, param, tr, commandTimeout, commandType))
            {
               using (var reader = cmd.ExecuteReader())
               {
                  foreach (
                      var result in
                          _dataReaderManager.ReadAll(reader, factory, allowUnbindableFetchResults,
                              allowUnbindableMembers)) yield return result;
               }
            }
            if (wasopen) yield break;
            skipClose = true;
            cn.Close();
         }
         finally
         {
            if (!wasopen && !skipClose)
            {
               cn.Close();
            }
         }
      }


      #region Private

      private IDbCommand CreateCommand(IDbConnection cn, string sql, object param, IDbTransaction tr,int? commandTimeout, CommandType? commandType)
      {
         return _commandFactory.CreateCommand(cn, sql, param, tr, commandTimeout, commandType);
      }

      private void FillParams(IDbCommand cmd, object param)
      {
         _commandFactory.FillParams(cmd, param);
      }


      private static void EnsureOpen(IDbConnection cn)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         cn.Close();
         cn.Open();
      }

      #endregion

   }
}
