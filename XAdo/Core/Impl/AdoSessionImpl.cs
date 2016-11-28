using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    // An ADO session hides connection (and transaction) management, and keeps a connection and its transaction together
    // if needed: the interface IAdoConnectionProvider exposes the inner connection and transaction

    public partial class AdoSessionImpl : IAdoSession, IAdoConnectionProvider, IAdoSessionInitializer
    {
       private readonly IAdoConnectionFactory
            _connectionFactory;

        private IDbTransaction
            _tr;

        private int?
            _commandTimeout;

        private bool
            _allowUnbindableFetchResults;

        private bool
            _allowUnbindableMembers;

        private readonly IAdoConnectionQueryManager
            _connectionQueryManager;

       private readonly IAdoClassBinder _binder;

       private IDbConnection _cn;

        private bool _keepConectionOpen;
        private string _connectionString;
        private string _providerName;

        private bool
            _disposed;

        private bool
            _autoCommit;

        private bool 
           _autoCommitSqlCommand;

       private readonly IDictionary<object, object> _items = new Dictionary<object, object>();

       private IDbConnection LazyInitializedConnection
        {
            get
            {
                if (_cn != null) return _cn;

                if (_connectionString == null)
                {
                    throw new AdoException("AdoSession has not been initialized yet.");
                }

                return _cn = _connectionFactory.CreateConnection(_connectionString, _providerName, _keepConectionOpen);
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

       public ISqlCommand SqlCommand { get; private set; }

       public IDictionary<object, object> Items
       {
          get { return _items; }
       }



       public virtual T ExecuteScalar<T>(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return _connectionQueryManager.ExecuteScalar<T>(LazyInitializedConnection, sql, param, _tr, _commandTimeout,
                commandType);
        }

        public virtual object ExecuteScalar(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return _connectionQueryManager.ExecuteScalar(LazyInitializedConnection, sql, param, _tr, _commandTimeout,
                commandType);
        }

        public virtual IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query<T>(LazyInitializedConnection, sql, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<T> Query<T>(string sql, Func<IDataRecord, T> factory, object param = null, bool buffered = true, CommandType? commandType = null)
       {
          EnsureNotDisposed();
          var enumerable = _connectionQueryManager.Query<T>(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout, commandType);
          return buffered ? enumerable.ToList() : enumerable;
       }

       public virtual IEnumerable<dynamic> Query(string sql, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, param, _tr, _commandTimeout,
                commandType);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual AdoMultiResultReader QueryMultiple(string sql, object param = null,CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return _connectionQueryManager.QueryMultiple(LazyInitializedConnection, sql, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);

        }

        public virtual AdoMultiResultReader QueryMultiple(string sql, IEnumerable<Delegate> factories, object param = null,
          CommandType? commandType = null)
       {
          EnsureNotDisposed();
          return _connectionQueryManager.QueryMultiple(LazyInitializedConnection, sql, factories, param, _tr, _commandTimeout, commandType);
       }

       public virtual IEnumerable<TResult> Query<T1, T2, TResult>(string sql, Func<T1, T2, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, TResult>(string sql, Func<T1, T2, T3, TResult> factory,
            object param = null, bool buffered = true, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(string sql,
            Func<T1, T2, T3, T4, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string sql,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, object param = null, bool buffered = true,
            CommandType? commandType = null)
        {
            EnsureNotDisposed();
            var enumerable = _connectionQueryManager.Query(LazyInitializedConnection, sql, factory, param, _tr, _commandTimeout,
                commandType, _allowUnbindableFetchResults, _allowUnbindableMembers);
            return buffered ? enumerable.ToList() : enumerable;
        }

        public virtual int Execute(string sql, object param = null, CommandType? commandType = null)
        {
            EnsureNotDisposed();
            return _connectionQueryManager.Execute(LazyInitializedConnection, sql, param, _tr, _commandTimeout, commandType);
        }

        #region Transaction

        public virtual bool HasTransaction
        {
            get
            {
                EnsureNotDisposed();
                return _tr != null;
            }
        }

        public virtual IAdoSession BeginTransaction(bool autoCommit = false)
        {
            EnsureNotDisposed();
            EnsureNoTransaction();
            EnsureConnectionIsOpen();
            _autoCommit = autoCommit;
            _tr = LazyInitializedConnection.BeginTransaction();
            return this;
        }

       public IAdoSession BeginSqlCommand(bool autoCommit = true)
       {
          _autoCommitSqlCommand = autoCommit;
          if (SqlCommand == null)
          {
             SqlCommand = _binder.Get<ISqlCommand>().Attach(this);
          }
          return this;
       }
       public IAdoSession CommitSqlCommand()
       {
          _autoCommitSqlCommand = false;
          var sqlcmd = SqlCommand;
          SqlCommand = null;
          if (sqlcmd != null)
          {
             sqlcmd.Flush();
          }
          return this;
       }
       public IAdoSession RollbackSqlCommand()
       {
          _autoCommitSqlCommand = false;
          var sqlcmd = SqlCommand;
          SqlCommand = null;
          if (sqlcmd != null)
          {
             sqlcmd.Clear();
          }
          return this;
       }


       public virtual bool Commit()
        {
            EnsureNotDisposed();
            _autoCommit = false;
            var hadWork = SqlCommand != null && SqlCommand.HasWork;
          CommitSqlCommand();
          var tr = _tr;
            _tr = null;
           if (tr == null) return hadWork;
            try
            {
                tr.Commit();
            }
            finally
            {
                tr.Dispose();
            }
            return true;
        }
        public virtual bool Rollback()
        {
            _autoCommit = false;
            var hadWork = SqlCommand != null && SqlCommand.HasWork;
           RollbackSqlCommand();
            var tr = _tr;
            _tr = null;
            if (tr == null) return hadWork;
            try
            {
                tr.Rollback();
            }
            finally
            {
                tr.Dispose();
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

          if (!HasTransaction && _autoCommitSqlCommand)
          {
             try
             {
                CommitSqlCommand();
             }
             finally
             {
                Dispose(true);
             }
             return;
          }

          if (HasTransaction && _autoCommit)
          {
             try
             {
                if (_autoCommitSqlCommand)
                {
                   CommitSqlCommand();
                }
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
            _cn = null;
            _tr = null;
            try
            {
                if (tr != null)
                {
                    tr.Rollback();
                }
            }
            finally
            {
                if (tr != null)
                {
                    tr.Dispose();
                }
                if (cn != null)
                {
                    cn.Dispose();
                }
            }
        }

        #endregion

        #region IAdoConnectionProvider

        public virtual IDbConnection Connection
        {
            get { return _cn; }
        }

        public virtual IDbTransaction Transaction
        {
            get { return _tr; }
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

        private void EnsureConnectionIsOpen()
        {
            if (LazyInitializedConnection.State != ConnectionState.Open)
            {
                LazyInitializedConnection.Close();
                LazyInitializedConnection.Open();
            }
        }

        #endregion

    }

}
