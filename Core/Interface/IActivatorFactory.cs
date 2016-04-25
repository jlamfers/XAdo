using System;

namespace XAdo.Core.Interface
{
    public interface IActivatorFactory
    {
        Func<object> GetActivator(Type type);
    }
}
