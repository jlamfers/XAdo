using System;
using System.Data;

namespace XAdo.Core.Interface
{
   public interface IXAdoContextInitializer
   {
      IXAdoContextInitializer SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> @delegate);
      IXAdoContextInitializer SetCustomDefaultTypeMapping(Type parameterType, DbType dbType);
      IXAdoContextInitializer SetCommandTimeout(int timeoutSeconds);
      IXAdoContextInitializer KeepConnectionAlive(bool value);
      IXAdoContextInitializer AllowUnbindableFetchResults(bool value);
      IXAdoContextInitializer AllowUnbindableProperties(bool value);
      IXAdoContextInitializer SetConnectionString(string connectionString, string providerName);
      IXAdoContextInitializer SetConnectionStringName(string connectionStringName);

      IXAdoContextInitializer Bind(Type serviceType, Type implementationType);
      IXAdoContextInitializer Bind(Type serviceType, Func<IXAdoClassBinder, object> factory);
      IXAdoContextInitializer BindSingleton(Type serviceType, Type implementationType);
      IXAdoContextInitializer BindSingleton(Type serviceType, Func<IXAdoClassBinder, object> factory);
      IXAdoContextInitializer SetItem(object key, object value);
      IXAdoContextInitializer OnInitialized(Action<XAdoDbContext> handler);
      bool CanResolve(Type serviceType);
   }

   public static partial class Extensions
   {
      public static IXAdoContextInitializer Bind<TService, TImpl>(this IXAdoContextInitializer self)
          where TImpl : TService
      {
         self.Bind(typeof(TService), typeof(TImpl));
         return self;
      }

      public static IXAdoContextInitializer Bind<TService>(this IXAdoContextInitializer self, Func<IXAdoClassBinder, TService> factory)
      {
         self.Bind(typeof(TService), b => factory(b));
         return self;
      }

      public static IXAdoContextInitializer BindSingleton<TService, TImpl>(this IXAdoContextInitializer self) where TImpl : TService
      {
         self.BindSingleton(typeof(TService), typeof(TImpl));
         return self;
      }
      public static IXAdoContextInitializer BindSingleton<TService>(this IXAdoContextInitializer self, Func<IXAdoClassBinder, TService> factory)
      {
         self.BindSingleton(typeof(TService), b => factory(b));
         return self;
      }
   }

}