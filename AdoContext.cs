﻿using System;
using System.Configuration;
using System.Data;
using XAdo.Core;
using XAdo.Core.Impl;
using XAdo.Core.Interface;

namespace XAdo
{
    public class AdoContext
    {
        private static AdoContext _defaultContext;

        private readonly IAdoClassBinder _binder = new AdoClassBinderImpl();

        private class Initializer : IAdoContextInitializer
        {
            private readonly AdoContext _context;

            public Initializer(AdoContext context)
            {
                _context = context;
            }

            public IAdoContextInitializer SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> @delegate)
            {
                if (@delegate == null) throw new ArgumentNullException("delegate");

                _context._binder
                    .Get<IAdoTypeConverterFactory>()
                    .SetCustomTypeConverter<TSource, TTarget>(@delegate);
                return this;
            }

            public IAdoContextInitializer SetCustomDefaultTypeMapping(Type parameterType, DbType dbType)
            {
                if (parameterType == null) throw new ArgumentNullException("parameterType");

                _context._binder
                   .Get<IAdoParameterFactory>()
                   .SetCustomDefaultTypeMapping(parameterType, dbType);
                return this;
            }

            public IAdoContextInitializer SetCommandTimeout(int timeoutSeconds)
            {
                if (timeoutSeconds < 0)
                {
                    throw new ArgumentException("timeoutSeconds must be >= 0", "timeoutSeconds");
                }
                _context.CommandTimeout = timeoutSeconds;
                return this;
            }

            public IAdoContextInitializer KeepConnectionAlive(bool value)
            {
                _context.KeepConnectionAlive = value;
                return this;
            }

            public IAdoContextInitializer AllowUnbindableFetchResults(bool value)
            {
                _context.AllowUnbindableFetchResults = value;
                return this;
            }

            public IAdoContextInitializer AllowUnbindableProperties(bool value)
            {
                _context.AllowUnbindableProperties = value;
                return this;
            }

            public IAdoContextInitializer SetConnectionString(string connectionString, string providerName)
            {
                if (connectionString == null) throw new ArgumentNullException("connectionString");
                if (providerName == null) throw new ArgumentNullException("providerName");
                _context.ConnectionString = connectionString;
                _context.ProviderName = providerName;
                return this;
            }

            public IAdoContextInitializer SetConnectionStringName(string connectionStringName)
            {
                if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");
                var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
                if (cs == null)
                {
                    throw new ConfigurationErrorsException("connectionStringName not found in configuration: " + connectionStringName);
                }
                _context.ConnectionStringName = connectionStringName;
                _context.ConnectionString = cs.ConnectionString;
                _context.ProviderName = cs.ProviderName;
                return this;
            }

            public IAdoContextInitializer Bind<TService, TImpl>() where TImpl : TService
            {
                _context._binder.Bind<TService, TImpl>();
                return this;
            }

            public IAdoContextInitializer BindSingleton<TService, TImpl>() where TImpl : TService
            {
                _context._binder.BindSingleton<TService, TImpl>();
                return this;
            }

            public IAdoContextInitializer Bind<TService>(Func<IAdoClassBinder, TService> factory)
            {
                if (factory == null) throw new ArgumentNullException("factory");
                _context._binder.Bind(factory);
                return this;
            }
        }

        public static AdoContext Default
        {
            get { return _defaultContext ?? (_defaultContext = new AdoContext()); }
            set { _defaultContext = value; }
        }

        public AdoContext()
        {
            Initialize(null);
        }

        public AdoContext(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
            Initialize(null);
        }

        public AdoContext(Action<IAdoContextInitializer> initializer, IAdoClassBinder customClassBinder = null)
        {
            _binder = customClassBinder ?? _binder;
            Initialize(initializer);
        }

        private void Initialize(Action<IAdoContextInitializer> initializer)
        {
            CommandTimeout = 30;
            AllowUnbindableFetchResults = true;

            TryBind(b => b);
            TryBindSingleton<IAdoDataBinderFactory, AdoDataBinderFactoryImpl>();
            TryBindSingleton<IAdoCommandFactory, AdoCommandFactoryImpl>();
            TryBindSingleton<IAdoConnectionFactory, AdoConnectionFactoryImpl>();
            TryBindSingleton<IAdoConnectionQueryManager, AdoConnectionQueryManagerImpl>();
            TryBindSingleton<IAdoDataReaderManager, AdoDataReaderManagerImpl>();
            TryBindSingleton<IAdoTypeConverterFactory, AdoTypeConverterFactoryImpl>();
            TryBindSingleton<IAdoMultiBinderFactory, AdoMultiBinderFactoryImpl>();
            TryBindSingleton<IAdoParameterFactory, AdoParameterFactoryImpl>();
            TryBindSingleton<IAdoSessionFactory, AdoSessionFactoryImpl>();
            TryBindSingleton<IConcreteTypeBuilder, ConcreteTypeBuilderImpl>();
            TryBindSingleton<IActivatorFactory, ActivatorFactoryImpl>();
            TryBind<IAdoParameter>(b => new AdoParameterImpl());
            TryBind<IAdoSession, AdoSessionImpl>();

            if (initializer != null)
            {
                initializer(new Initializer(this));
            }

            AdoParamHelper = new AdoParamHelper(_binder.Get<IAdoParameterFactory>());
        }

        private void TryBindSingleton<TService, TImpl>() where TImpl : TService
        {
            if (!_binder.CanResolve<TService>())
            {
                _binder.BindSingleton<TService, TImpl>();
            }
        }
        private void TryBind<TService, TImpl>() where TImpl : TService
        {
            if (!_binder.CanResolve<TService>())
            {
                _binder.Bind<TService, TImpl>();
            }
        }
        private void TryBind<TService>(Func<IAdoClassBinder, TService> factory)
        {
            if (!_binder.CanResolve<TService>())
            {
                _binder.Bind(factory);
            }
        }

        public virtual IAdoSession CreateSession()
        {
            var check = string.Format("{0}{1}", ConnectionString, ConnectionStringName).Trim();

            if (check.Length == 0)
            {
                throw new InvalidOperationException("Both ConnectionStringName and ConnectionString haven not been initialized by the constructor.");
            }

            return ConnectionStringName != null 
                ? GetInstance<IAdoSessionFactory>().Create(ConnectionStringName, CommandTimeout, KeepConnectionAlive, AllowUnbindableFetchResults, AllowUnbindableProperties)
                : GetInstance<IAdoSessionFactory>().Create(ConnectionString, ProviderName, CommandTimeout, KeepConnectionAlive, AllowUnbindableFetchResults, AllowUnbindableProperties);
        }

        public AdoParamHelper AdoParamHelper { get; private set; }

        public virtual TService GetInstance<TService>()
        {
            return _binder.Get<TService>();
        }

        public string ConnectionStringName { get; protected set; }
        public int CommandTimeout { get; protected set; }
        public bool KeepConnectionAlive { get; protected set; }
        public bool AllowUnbindableFetchResults { get; protected set; }
        public bool AllowUnbindableProperties { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string ProviderName { get; protected set; }
    }
}
