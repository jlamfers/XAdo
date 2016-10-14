using System;
using System.Configuration;
using System.Data;
using XAdo.Core;
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
   public class AdoContext
   {
      private readonly IAdoClassBinder _binder = new AdoClassBinderImpl();

      private class ContextInitializer : IAdoContextInitializer
      {
         private readonly AdoContext _context;

         public ContextInitializer(AdoContext context)
         {
            _context = context;
         }

         public IAdoContextInitializer SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> @delegate)
         {
            if (@delegate == null) throw new ArgumentNullException("delegate");

            _context._binder
                .Get<IAdoTypeConverterFactory>()
                .SetCustomTypeConverter(@delegate);
            return this;
         }

         public bool CanCustomConvert(Type sourceType, Type targetType)
         {
            if (sourceType == null) throw new ArgumentNullException("sourceType");
            if (targetType == null) throw new ArgumentNullException("targetType");

            return _context._binder
                .Get<IAdoTypeConverterFactory>()
                .CanCustomConvert(sourceType, targetType);
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

         public IAdoContextInitializer Bind(Type serviceType, Type implementationType)
         {
            _context._binder.Bind(serviceType, implementationType);
            return this;
         }

         public IAdoContextInitializer BindSingleton(Type serviceType, Type implementationType)
         {
            _context._binder.BindSingleton(serviceType, implementationType);
            return this;
         }

         public IAdoContextInitializer Bind(Type serviceType, Func<IAdoClassBinder, object> factory)
         {
            if (factory == null) throw new ArgumentNullException("factory");
            _context._binder.Bind(serviceType, factory);
            return this;
         }
      }

      public AdoContext(string connectionStringName)
      {
         if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");
         ConnectionStringName = connectionStringName;
         Initialize(null);
      }

      public AdoContext(Action<IAdoContextInitializer> initializer, IAdoClassBinder customClassBinder = null)
      {
         if (initializer == null) throw new ArgumentNullException("initializer");

         // allow to use a custom class binder (container)
         _binder = customClassBinder ?? _binder;

         Initialize(initializer);
      }

      private void Initialize(Action<IAdoContextInitializer> initializer)
      {
         CommandTimeout = 30;
         AllowUnbindableFetchResults = true;

         // bind any type that has no binding yet. It may have been bound by any custom class binder
         TryBind(b => b);
         TryBindSingleton<IAdoDataBinderFactory, AdoDataBinderFactoryImpl>();
         TryBindSingleton<IAdoCommandFactory, AdoCommandFactoryImpl>();
         TryBindSingleton<IAdoConnectionFactory, AdoConnectionFactoryImpl>();
         TryBindSingleton<IAdoConnectionQueryManager, AdoConnectionQueryManagerImpl>();
         TryBindSingleton<IAdoDataReaderManager, AdoDataReaderManagerImpl>();
         TryBindSingleton<IAdoTypeConverterFactory, AdoTypeConverterFactoryImpl>();
         TryBindSingleton<IAdoGraphBinderFactory, AdoGraphBinderFactoryImpl>();
         TryBindSingleton<IAdoParameterFactory, AdoParameterFactoryImpl>();
         TryBindSingleton<IAdoSessionFactory, AdoSessionFactoryImpl>();
         TryBindSingleton<IConcreteTypeBuilder, ConcreteTypeBuilderImpl>();
         TryBind<IAdoParameter>(b => new AdoParameterImpl());
         TryBind<IAdoSession, AdoSessionImpl>();
         TryBind(typeof(IGetterFactory<,>), typeof(GetterFactory<,>));

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
         if (!contextInitializer.CanCustomConvert(typeof(int), typeof(decimal)))
         {
            // add default int to decimal converter
            contextInitializer.SetCustomTypeConverter<int, decimal>(x => 0M + x);
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
      private void TryBind(Type serviceType, Type implementationType)
      {
         if (!_binder.CanResolve(serviceType))
         {
            _binder.Bind(serviceType, implementationType);
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
            throw new InvalidOperationException("Both ConnectionStringName and ConnectionString have not been initialized by the constructor.");
         }

         return ConnectionStringName != null
             ? GetInstance<IAdoSessionFactory>().Create(ConnectionStringName, CommandTimeout, KeepConnectionAlive, AllowUnbindableFetchResults, AllowUnbindableProperties)
             : GetInstance<IAdoSessionFactory>().Create(ConnectionString, ProviderName, CommandTimeout, KeepConnectionAlive, AllowUnbindableFetchResults, AllowUnbindableProperties);
      }

      public virtual AdoParamHelper AdoParamHelper { get; private set; }

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
   }
}
