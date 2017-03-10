using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using XAdo.Core.Impl;
using XAdo.Core.Interface;

namespace XAdo
{
   /// <summary>
   /// The AdoContext class represents a database access configuration context. It also hold the context related cache
   /// for reader/binder setups.
   /// Normally you would create a singleton context per database configuration, and use that context each time
   /// you need to create a session for executing SQL.
   /// </summary>
   /// <example>
   ///     // create context with default settings using connection string named "AdventureWorks"
   ///     var context = new AdoContext("AdventureWorks");
   /// 
   ///     // create context with customized settings
   ///     var context = new AdoContext(i => i
   ///         .SetConnectionStringName("AdventureWorks")
   ///         .KeepConnectionAlive(true)
   ///         .SetCommandTimeout(90)
   ///         .SetCustomDefaultTypeMapping(typeof(string), DbType.AnsiString)
   ///         .SetCustomTypeConverter&lt;string, bool&gt;(s => s == null ? false : bool.Parse(s))
   ///         .EnableEmittedDynamicTypes()
   ///     );
   /// </example>
   public class XAdoDbContext
   {
      private readonly IXAdoClassBinder _binder = new XAdoClassBinderImpl();

      private class ContextInitializer : IXAdoContextInitializer
      {
         private readonly XAdoDbContext _context;
         private readonly IList<Action<XAdoDbContext>> _initializeCompletedHandlers = new List<Action<XAdoDbContext>>();

         public ContextInitializer(XAdoDbContext context)
         {
            _context = context;
         }

         public IXAdoContextInitializer SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> @delegate)
         {
            if (@delegate == null) throw new ArgumentNullException("delegate");

            _context._binder
                .Get<IXAdoTypeConverterFactory>()
                .SetCustomTypeConverter(@delegate);
            return this;
         }

         public bool CanCustomConvert(Type sourceType, Type targetType)
         {
            if (sourceType == null) throw new ArgumentNullException("sourceType");
            if (targetType == null) throw new ArgumentNullException("targetType");

            return _context._binder
                .Get<IXAdoTypeConverterFactory>()
                .CanCustomConvert(sourceType, targetType);
         }

         public IXAdoContextInitializer SetCustomDefaultTypeMapping(Type parameterType, DbType dbType)
         {
            if (parameterType == null) throw new ArgumentNullException("parameterType");

            _context._binder
               .Get<IXAdoParameterFactory>()
               .SetCustomDefaultTypeMapping(parameterType, dbType);
            return this;
         }

         public IXAdoContextInitializer SetCommandTimeout(int timeoutSeconds)
         {
            if (timeoutSeconds < 0)
            {
               throw new ArgumentException("timeoutSeconds must be >= 0", "timeoutSeconds");
            }
            _context.CommandTimeout = timeoutSeconds;
            return this;
         }

         public IXAdoContextInitializer KeepConnectionAlive(bool value)
         {
            _context.KeepConnectionAlive = value;
            return this;
         }

         public IXAdoContextInitializer AllowUnbindableFetchResults(bool value)
         {
            _context.AllowUnbindableFetchResults = value;
            return this;
         }

         public IXAdoContextInitializer AllowUnbindableProperties(bool value)
         {
            _context.AllowUnbindableProperties = value;
            return this;
         }

         public IXAdoContextInitializer SetConnectionString(string connectionString, string providerName)
         {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (providerName == null) throw new ArgumentNullException("providerName");
            _context.ConnectionString = connectionString;
            _context.ProviderName = providerName;
            return this;
         }

         public IXAdoContextInitializer SetConnectionStringName(string connectionStringName)
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

         public IXAdoContextInitializer Bind(Type serviceType, Type implementationType)
         {
            _context._binder.Bind(serviceType, implementationType);
            return this;
         }

         public IXAdoContextInitializer BindSingleton(Type serviceType, Type implementationType)
         {
            _context._binder.BindSingleton(serviceType, implementationType);
            return this;
         }

         public IXAdoContextInitializer BindSingleton(Type serviceType, Func<IXAdoClassBinder, object> factory)
         {
            _context._binder.BindSingleton(serviceType, factory);
            return this;
         }

         public IXAdoContextInitializer SetItem(object key, object value)
         {
            _context.Items[key] = value;
            return this;
         }

         public IXAdoContextInitializer SetSqlStatementSeperator(string seperator)
         {
            _context.SqlStatementSeperator = seperator;
            return this;
         }

         public IXAdoContextInitializer OnInitialized(Action<XAdoDbContext> handler)
         {
            _initializeCompletedHandlers.Add(handler);
            return this;
         }

         public bool CanResolve(Type serviceType)
         {
            return _context._binder.CanResolve(serviceType);
         }

         public IXAdoContextInitializer Bind(Type serviceType, Func<IXAdoClassBinder, object> factory)
         {
            if (factory == null) throw new ArgumentNullException("factory");
            _context._binder.Bind(serviceType, factory);
            return this;
         }

         public void Initialized(XAdoDbContext context)
         {
            foreach (var handler in _initializeCompletedHandlers)
            {
               handler(context);
            }
         }
      }

      public XAdoDbContext(string connectionStringName)
      {
         Items = new Dictionary<object, object>();
         if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");
         ConnectionStringName = connectionStringName;
         var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
         if (cs != null)
         {
            ConnectionString = cs.ConnectionString;
            ProviderName = cs.ProviderName;
         }
         Initialize(null);

      }

      public XAdoDbContext(Action<IXAdoContextInitializer> initializer, IXAdoClassBinder customClassBinder = null)
      {
         Items = new Dictionary<object, object>();
         if (initializer == null) throw new ArgumentNullException("initializer");

         // allow to use a custom class binder (container)
         _binder = customClassBinder ?? _binder;

         Initialize(initializer);
      }

      private void Initialize(Action<IXAdoContextInitializer> initializer)
      {
         CommandTimeout = 30;
         AllowUnbindableFetchResults = true;
         SqlStatementSeperator = ";";

         // bind any type that has no binding yet. It may have been bound by any custom class binder
         TryBind(b => b);
         TryBind(b => this);
         TryBindSingleton<IXAdoDataBinderFactory, XAdoDataBinderFactoryImpl>();
         TryBindSingleton<IXAdoCommandFactory, XAdoCommandFactoryImpl>();
         TryBindSingleton<IXAdoConnectionFactory, XAdoConnectionFactoryImpl>();
         TryBindSingleton<IXAdoConnectionQueryManager, XAdoConnectionQueryManagerImpl>();
         TryBindSingleton<IXAdoDataReaderManager, XAdoDataReaderManagerImpl>();
         TryBindSingleton<IXAdoTypeConverterFactory, XAdoTypeConverterFactoryImpl>();
         TryBindSingleton<IXAdoGraphBinderFactory, XAdoGraphBinderFactoryImpl>();
         TryBindSingleton<IXAdoParameterFactory, XAdoParameterFactoryImpl>();
         TryBindSingleton<IXAdoDbSessionFactory, XAdoDbSessionFactoryImpl>();
         TryBindSingleton<IXAdoConcreteTypeBuilder, XAdoConcreteTypeBuilderImpl>();
         TryBindSingleton(typeof(IGetterFactory<,>), typeof(GetterFactory<,>));
         TryBindSingleton<IXAdoParamHelper, XAdoParamHelperImpl>();
         TryBind<IXAdoSqlBatch>(b => new XAdoSqlBatchImpl{Seperator = SqlStatementSeperator});
         TryBind<IXAdoParameter>(b => new XAdoParameterImpl());
         TryBind<IXAdoDbSession, XAdoDbSessionImpl>();

         var contextInitializer = new ContextInitializer(this);
         if (initializer != null)
         {
            initializer(contextInitializer);
         }

         if (!contextInitializer.CanCustomConvert(typeof(int), typeof(long)))
         {
            // add default int to long converter
            contextInitializer.SetCustomTypeConverter<int, long>(x => 0L + x);
         }
         if (!contextInitializer.CanCustomConvert(typeof(int), typeof(float)))
         {
            // add default int to float converter
            contextInitializer.SetCustomTypeConverter<int, float>(x => 0F + x);
         }
         if (!contextInitializer.CanCustomConvert(typeof(int), typeof(double)))
         {
            // add default int to double converter
            contextInitializer.SetCustomTypeConverter<int, double>(x => 0D + x);
         }
         if (!contextInitializer.CanCustomConvert(typeof(int), typeof(decimal)))
         {
            // add default int to decimal converter
            contextInitializer.SetCustomTypeConverter<int, decimal>(x => 0M + x);
         }

         AdoParamHelper = _binder.Get<IXAdoParamHelper>();

         Items = new ReadOnlyDictionary<object, object>(Items);

         contextInitializer.Initialized(this);

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
      private void TryBind(Type serviceType, Type implementationType)
      {
         if (!_binder.CanResolve(serviceType))
         {
            _binder.Bind(serviceType, implementationType);
         }
      }
      private void TryBindSingleton(Type serviceType, Type implementationType)
      {
         if (!_binder.CanResolve(serviceType))
         {
            _binder.BindSingleton(serviceType, implementationType);
         }
      }
      private void TryBind<TService>(Func<IXAdoClassBinder, TService> factory)
      {
         if (!_binder.CanResolve<TService>())
         {
            _binder.Bind(factory);
         }
      }

      public IDictionary<object, object> Items { get; private set; }

      public virtual IXAdoDbSession CreateSession()
      {
         var check = string.Format("{0}{1}", ConnectionString, ConnectionStringName).Trim();

         if (check.Length == 0)
         {
            throw new InvalidOperationException("Both ConnectionStringName and ConnectionString have not been initialized by the constructor.");
         }

         return ConnectionStringName != null
             ? GetInstance<IXAdoDbSessionFactory>().Create(ConnectionStringName, CommandTimeout, KeepConnectionAlive, AllowUnbindableFetchResults, AllowUnbindableProperties)
             : GetInstance<IXAdoDbSessionFactory>().Create(ConnectionString, ProviderName, CommandTimeout, KeepConnectionAlive, AllowUnbindableFetchResults, AllowUnbindableProperties);
      }

      public virtual IXAdoParamHelper AdoParamHelper { get; private set; }

      /// <summary>
      /// You may resolve instances from the inner container
      /// </summary>
      /// <typeparam name="TService"></typeparam>
      /// <returns></returns>
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
      public string SqlStatementSeperator { get; protected set; }
   }
}
