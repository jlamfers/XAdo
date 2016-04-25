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

        IAdoContextInitializer Bind<TService, TImpl>() where TImpl : TService;
        IAdoContextInitializer Bind<TService>(Func<IAdoClassBinder, TService> factory);
        IAdoContextInitializer BindSingleton<TService, TImpl>() where TImpl : TService;
    }
}