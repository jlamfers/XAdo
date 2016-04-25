using System;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class ActivatorFactoryImpl : IActivatorFactory
    {

        public virtual Func<object> GetActivator(Type type)
        {
            return () => Activator.CreateInstance(type);
        }
    }
}
