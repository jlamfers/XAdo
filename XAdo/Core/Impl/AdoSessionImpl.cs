using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    // An ADO session hides connection (and transaction, and sql queue) management, and keeps a connection and its transaction together
    // if needed: the interface IAdoConnectionProvider exposes the inner connection and transaction
   //  both connection and transaction are created on first need (lazy)

    public partial class AdoSessionImpl : IAdoSession, IAdoConnectionProvider, IAdoSessionInitializer
    {
       private readonly IAdoConnectionFactory
            _connectionFactory;

       private Lazy<IDbConnection> _cn;
       private Lazy<IDbTransaction> _tr;

        private int?
            _commandTimeout;

        private bool
            _allowUnbindableFetchResults;

        private bool
            _allowUnbindableMembers;

        private readonly IAdoConnectionQueryManager
            _connectionQueryManager;

       private readonly IAdoClassBinder _binder;

       private bool _keepConectionOpen;
        private string _connectionString;
        private string _providerName;

        private bool
            _disposed;

        private bool
            _autoCommit;

        private IAdoSqlBatch 
           _sqlBatch;


       private readonly IDictionary<object, object> _items = new Dictionary<object, object>();

       private Lazy<IDbConnection> LazyConnection
        {
           [MethodImpl(MethodImplOptions.NoOptimization)]
           get
            {
                if (_cn != null) return _cn;

                if (_connectionString == null)
                {
                    throw new AdoException("AdoSession has not been initialized yet.");
                }

               _cn = new Lazy<IDbConnection>(() =>
               {
                  var cn = _connectionFactory.CreateConnection(_connectionString, _providerName, _keepConectionOpen);
                  return cn;
               }
                  );

               if (_tr != null)
               {
                  _tr.Value.GetType();
               }

               return _cn;
            }
        }


        public AdoSessionImpl(IAdoConnectionFactory connectionFactory, IAdoConnectionQueryManager connectionQueryManager, AdoContext context, IAdoClassBinder binder)
        {
           if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");
            if (connectionQueryManager == null) throw new ArgumentNullException("connectionQueryManager");
           if (context == null) throw new ArgumentNullException("context");
           if (binder == null) throw new ArgumentNullException("binder");

           _connectionFactory = connectionFactory;
            _connectionQueryManager = connectionQueryManager;
           _binder = binder;
           Context = context;
        }

        public virtual IAdoSession Initialize(string connectionStringName, int? commandTimeout = null,
            bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null)
            {
                throw new ConfigurationErrorsException("Connection string name " + connectionStringName + " not found.");
            }
            return Initialize(cs.ConnectionString, cs.ProviderName, commandTimeout, keepConnectionOpen,
                allowUnbindableFetchResults, allowUnbindableMembers);
        }

        public virtual IAdoSession Initialize(string connectionString, string providerName, int? commandTimeout = null,
            bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true,
            bool allowUnbindableMembers = false)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (providerName == null) throw new ArgumentNullException("providerName");
            EnsureNotDisposed();
            if (_cn != null)
            {
                throw new AdoException("AdoSession has been initialized already");
            }
            _commandTimeout = commandTimeout;
            _allowUnbindableFetchResults = allowUnbindableFetchResults;
            _allowUnbindableMembers = allowUnbindableMembers;
            _keepConectionOpen = keepConnectionOpen;
            _connectionString = connectionString;
            _providerName = providerName;
            return this;
        }

        public AdoContext Context { get; private set; }


       public IDictionary<object, object> Items
       {
          get { return _items; }
       }



       public virtual T ExecuteScalar<T>(string sql, object param = null, CommandType? commandType = null)
       {
          EnsureNotDisposed();

          return _connectionQueryManager.ExecuteScalar<T>(LazyConnection.Value, sql, param,
             _tr != null ? _tr.Value : null, _commandTimeout,
             commandType);
       }

       public virtual object ExecuteScalar(string sql, object param = null, CommandType? commandType = null)
        {
           EnsureNotDisposed();

            return _connectionQueryManager.ExecuteScalar(LazyConnection.Value, sql, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType);
        }

        public virtual IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, CommandType? commandType = null)
        {
           EnsureNotDisposed();

            var enumerable = _connectionQueryManager.Query<T>(LazyConnection.Value, sql, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<T> Query<T>(string sql, Func<IDataRecord, T> factory, object param = null, bool buffered = true, CommandType? commandType = null)
       {
          EnsureNotDisposed();

          var enumerable = _connectionQueryManager.Query<T>(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout, commandType);
          return buffered ? enumerable.ToList() : enumerable;
       }

       public virtual IEnumerable<dynamic> Query(string sql, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();

            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual AdoMultiResultReader QueryMultiple(string sql, object param = null,CommandType? commandType = null)
        {
            EnsureNotDisposed();

            return _connectionQueryManager.QueryMultiple(LazyConnection.Value, sql, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);

        }

        public virtual AdoMultiResultReader QueryMultiple(string sql, IEnumerable<Delegate> factories, object param = null,
          CommandType? commandType = null)
       {
          EnsureNotDisposed();

          return _connectionQueryManager.QueryMultiple(LazyConnection.Value, sql, factories, param, _tr != null ? _tr.Value : null, _commandTimeout, commandType);
       }

       public virtual IEnumerable<TResult> Query<T1, T2, TResult>(string sql, Func<T1, T2, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, TResult>(string sql, Func<T1, T2, T3, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(string sql,
            Func<T1, T2, T3, T4, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyConnection.Value, sql, factory, param, _tr != null ? _tr.Value : null, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual int Execute(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return _connectionQueryManager.Execute(LazyConnection.Value, sql, param, _tr != null ? _tr.Value : null, _commandTimeout, commandType);
        }


       #region SqlBatch
        public virtual bool HasSqlBatch
        {
           get
           {
              EnsureNotDisposed();
              return _sqlBatch != null;
           }
        }
        public virtual bool StartSqlBatch()
       {
          EnsureNotDisposed();
          if (_sqlBatch != null) return true;
          _sqlBatch = _binder.Get<IAdoSqlBatch>();
          return true;
       }
       public virtual bool StopSqlBatch()
       {
          EnsureNotDisposed();
          var batch = _sqlBatch;
          _sqlBatch = null;
          if (batch == null || batch.Count == 0) return false;
          batch.Flush(this);
          return true;
       }
       public virtual bool FlushSqlBatch()
       {
          EnsureNotDisposed();
          var batch = _sqlBatch;
          _sqlBatch = null;
          if (batch == null || batch.Count <= 0) return false;
          batch.Flush(this);
          _sqlBatch = batch;
          return true;
       }
       public virtual IAdoSession AddSqlBatchItem(AdoSqlBatchItem batchItem)
       {
          EnsureNotDisposed();
          var batch = _sqlBatch;
          EnsureSqlBatch();
          batch.Add(batchItem);
          return this;
       }
       #endregion

       #region Transaction
       public virtual bool HasTransaction
       {
          get
          {
             EnsureNotDisposed();
             return _tr != null;
          }
       }
       public virtual IAtomic BeginTransaction(bool autoCommit = false)
        {
            EnsureNotDisposed();
            EnsureNoTransaction();
            _autoCommit = autoCommit;
            _tr = new Lazy<IDbTransaction>(() => EnsureConnectionIsOpen(LazyConnection.Value).BeginTransaction());
            return new Atomic(Commit, Rollback);
        }
       protected virtual bool Commit()
       {
          EnsureNotDisposed();
          _autoCommit = false;
          var hadWork = _sqlBatch != null && _sqlBatch.Count > 0;
          FlushSqlBatch();
          var tr = _tr;
          _tr = null;
          if (tr == null || !tr.IsValueCreated) return hadWork;
          try
          {
             tr.Value.Commit();
          }
          finally
          {
             tr.Value.Dispose();
          }
          return true;
       }
       protected virtual bool Rollback()
       {
          _autoCommit = false;
          var batch = _sqlBatch;
          var hadWork = batch != null && batch.Count > 0;
          if (hadWork) batch.Clear();
          var tr = _tr;
          _tr = null;
          if (tr == null || !tr.IsValueCreated) return hadWork;
          try
          {
             tr.Value.Rollback();
          }
          finally
          {
             tr.Value.Dispose();
          }
          return true;
       }
       #endregion

        #region IDispose

       public void Dispose()
       {
          var hadException = Marshal.GetExceptionCode() != 0;
          if (hadException)
          {
             Dispose(true);
             return;
          }
          if (!HasTransaction)
          {
             StopSqlBatch();
          }

          if (HasTransaction && _autoCommit)
          {
             try
             {
                StopSqlBatch();
                Commit();
             }
             finally
             {
                Dispose(true);
             }
             return;
          }
          Dispose(true);
       }

       protected virtual void Dispose(bool disposing)
       {
          if (_disposed) return;
          _disposed = true;

          if (!disposing) return;

          var cn = _cn;
          var tr = _tr;
          var batch = _sqlBatch;
          _cn = null;
          _tr = null;
          _sqlBatch = null;
          if (batch != null)
          {
             batch.Clear();
          }
          try
          {
             if (tr != null && tr.IsValueCreated)
             {
                tr.Value.Rollback();
             }
          }
          finally
          {
             if (tr != null && tr.IsValueCreated)
             {
                tr.Value.Dispose();
             }
             if (cn != null && cn.IsValueCreated)
             {
                cn.Value.Dispose();
             }
          }
       }

       #endregion

        #region IAdoConnectionProvider

        public virtual IDbConnection Connection
        {
           get { return LazyConnection.Value; }
        }
        public virtual IDbTransaction Transaction
        {
           get { return _tr == null ? null : _tr.Value; }
        }

        #endregion

        #region Validation

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void EnsureNoTransaction()
        {
            if (_tr != null)
            {
                throw new AdoException("Transaction cannot be started more than once");
            }
        }

        private void EnsureSqlBatch()
        {
           if (_sqlBatch == null)
           {
              throw new AdoException("No SQL batch available. SQL cannot be batched");
           }
        }

        private IDbConnection EnsureConnectionIsOpen(IDbConnection cn)
        {
           if (cn.State != ConnectionState.Open)
           {
              cn.Close();
              cn.Open();
           }
           return cn;
        }

        #endregion

    }

}
