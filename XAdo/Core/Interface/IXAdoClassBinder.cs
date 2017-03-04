using System;

namespace XAdo.Core.Interface
{
    public interface IXAdoClassBinder 
    {
        IXAdoClassBinder Bind(Type serviceType, Func<IXAdoClassBinder,object> factory);
        IXAdoClassBinder Bind(Type serviceType, Type implementationType);
        IXAdoClassBinder BindSingleton(Type serviceType, Type implementationType);
        IXAdoClassBinder BindSingleton(Type serviceType, Func<IXAdoClassBinder, object> factory);
        bool CanResolve(Type serviceType);
        object Get(Type serviceType);
    }
}