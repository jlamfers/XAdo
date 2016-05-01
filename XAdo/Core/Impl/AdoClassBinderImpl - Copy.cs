using System;
using System.Collections.Concurrent;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    // This basic IOC implementation provides enough to serve this framework's needs.
    // It uses any first ctor for instantiation. All ctor parameter types must 
    // be bindable (registered) parameter types. Default parameters are not supported,
    // nor scope management, nor multiple same service type registrations. 

    public class AdoClassBinderImpl : IAdoClassBinder
    {
        private readonly ConcurrentDictionary<Type, Type>
            _bindings = new ConcurrentDictionary<Type, Type>();

        private readonly ConcurrentDictionary<Type, Func<object>>
            _factories = new ConcurrentDictionary<Type, Func<object>>();

        private bool
            _readOnly;

        public virtual IAdoClassBinder Bind<TService>(Func<IAdoClassBinder, TService> factory)
        {
            _factories[typeof (TService)] = () => factory(this);
            return this;
        }

        public virtual IAdoClassBinder Bind<TService, TImpl>() 
            where TImpl : TService
        {
            EnsureNotReadonly();
            _bindings[typeof (TService)] = typeof (TImpl);
            Func<object> removed;
            _factories.TryRemove(typeof(TService), out removed);
            return this;
        }

        public virtual IAdoClassBinder BindSingleton<TService, TImpl>() where TImpl : TService
        {
            EnsureNotReadonly();
            Bind<TService, TImpl>();
            var fact = GetOrAddFactory(typeof(TService));
            object singleton = null;
            _factories[typeof(TService)] = () => singleton ?? (singleton = fact());
            return this;
        }

        public virtual bool CanResolve<TService>()
        {
            return _factories.ContainsKey(typeof (TService));
        }

        public virtual T Get<T>()
        {
            return (T)Resolve(typeof (T));
        }

        private object Resolve(Type type)
        {
            _readOnly = true;
            return GetOrAddFactory(type)();
        }

        private Func<object> GetOrAddFactory(Type type)
        {
            return _factories.GetOrAdd(type, t =>
            {
                var implType = _bindings[t];
                var ctor = implType.GetConstructors()[0];
                return ctor.GetParameters().Length == 0
                    ? new Func<object>(() => Activator.CreateInstance(implType))
                    : () => ctor.Invoke(ctor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray());
            });
        }

        private void EnsureNotReadonly()
        {
            if (_readOnly)
            {
                throw new AdoException("You cannot bind additional (nor replacing) service types after you have resolved any service type. So make sure you bind all needed types ASAP, before any service type could have been resolved. Always put your bind statements on top inside your initializer.");
            }
        }

    }
}
