using System;

namespace XAdo.Core.Interface
{
    public interface IAdoClassBinder 
    {
        IAdoClassBinder Bind<TService>(Func<IAdoClassBinder,TService> factory);
        IAdoClassBinder Bind<TService, TImpl>() where TImpl : TService;
        IAdoClassBinder BindSingleton<TService, TImpl>() where TImpl : TService;
        bool CanResolve<TService>();
        T Get<T>();
    }
}