using System;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoContextInitializer
    {
        IAdoContextInitializer SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> @delegate);
        IAdoContextInitializer SetCustomDefaultTypeMapping(Type parameterType, DbType dbType);
        IAdoContextInitializer SetCommandTimeout(int timeoutSeconds);
        IAdoContextInitializer KeepConnectionAlive(bool value);
        IAdoContextInitializer AllowUnbindableFetchResults(bool value);
        IAdoContextInitializer AllowUnbindableProperties(bool value);
        IAdoContextInitializer SetConnectionString(string connectionString, string providerName);
        IAdoContextInitializer SetConnectionStringName(string connectionStringName);

        IAdoContextInitializer Bind(Type serviceType, Type implementationType);
        IAdoContextInitializer Bind(Type serviceType, Func<IAdoClassBinder, object> factory);
        IAdoContextInitializer BindSingleton(Type serviceType, Type implementationType);
        IAdoContextInitializer BindSingleton(Type serviceType, Func<IAdoClassBinder, object> factory);
        IAdoContextInitializer SetItem(object key, object value);
        IAdoContextInitializer SetSqlStatementSeperator(string seperator);
    }

    public static partial class Extensions
    {
        public static IAdoContextInitializer Bind<TService, TImpl>(this IAdoContextInitializer self)
            where TImpl : TService
        {
            self.Bind(typeof (TService), typeof (TImpl));
            return self;
        }

        public static IAdoContextInitializer Bind<TService>(this IAdoContextInitializer self, Func<IAdoClassBinder, TService> factory)
        {
            self.Bind(typeof(TService), b => factory(b));
            return self;
        }

        public static IAdoContextInitializer BindSingleton<TService, TImpl>(this IAdoContextInitializer self) where TImpl : TService
        {
            self.BindSingleton(typeof(TService), typeof(TImpl));
            return self;
        }
        public static IAdoContextInitializer BindSingleton<TService>(this IAdoContextInitializer self, Func<IAdoClassBinder, TService> factory)
        {
           self.BindSingleton(typeof(TService), b => factory(b));
           return self;
        }
    }
}