using System;

namespace XAdo.Core.Interface
{
    public interface IAdoClassBinder 
    {
        IAdoClassBinder Bind(Type serviceType, Func<IAdoClassBinder,object> factory);
        IAdoClassBinder Bind(Type serviceType, Type implementationType);
        IAdoClassBinder BindSingleton(Type serviceType, Type implementationType);
        bool CanResolve(Type serviceType);
        object Get(Type serviceType);
    }
}