using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    // This basic IOC implementation provides enough to serve this framework's needs.
    // It supports transient and singleton registrations. Open generic types, as well
    // as default arguments are supported.
    // Scope management is not supported, nor multiple same service type registrations. 

    public class AdoClassBinderImpl : IAdoClassBinder
    {
        private readonly ConcurrentDictionary<Type, Type>
            _bindings = new ConcurrentDictionary<Type, Type>();

        private readonly ConcurrentDictionary<Type, Func<object>>
            _factories = new ConcurrentDictionary<Type, Func<object>>();

        private bool
            _readOnly;

        private readonly HashSet<Type>
            _singletons = new HashSet<Type>();

        public virtual IAdoClassBinder Bind(Type serviceType, Func<IAdoClassBinder,object> factory)
        {
            _factories[serviceType] = () => factory(this);
            return this;
        }
        public virtual IAdoClassBinder Bind(Type serviceType, Type implementationType)
        {
            EnsureNotReadonly();
            _bindings[serviceType] = implementationType;
            Func<object> factory;
            _factories.TryRemove(serviceType, out factory);
            _singletons.Remove(serviceType);
            return this;
        }
        public virtual IAdoClassBinder BindSingleton(Type serviceType, Type implementationType)
        {
            Bind(serviceType,implementationType);
            _singletons.Add(serviceType);
            return this;
        }
        public virtual bool CanResolve(Type serviceType)
        {
            return _bindings.ContainsKey(serviceType) || _factories.ContainsKey(serviceType) || (serviceType.IsGenericType && _bindings.ContainsKey(serviceType.GetGenericTypeDefinition()));
        }

        public virtual object Get(Type serviceType)
        {
            return Resolve(serviceType);
        }

        #region Private
        private object Resolve(Type type)
        {
            _readOnly = true;
            return GetOrAddFactory(type)();
        }

        private Func<object> GetOrAddFactory(Type type)
        {
            return _factories.GetOrAdd(type, t =>
            {
                Type implType;
                var singleton = _singletons.Contains(t);
                if (!_bindings.TryGetValue(t, out implType) && t.IsGenericType)
                {
                    var t2 = t.GetGenericTypeDefinition();
                    if (_bindings.TryGetValue(t2, out implType))
                    {
                        singleton = _singletons.Contains(t2);
                        implType = implType.MakeGenericType(t.GetGenericArguments());
                    }
                }
                if (implType == null)
                {
                    throw new AdoException(string.Format("Type '{0}' could not be resolved. It was not registered.",t));
                }
                var ctor = implType.GetConstructors()
                    .Where(c => c.GetParameters().Length == 0 || c.GetParameters().All(p => CanResolve(p.ParameterType) || p.HasDefaultValue))
                    .OrderByDescending(p => p.GetParameters().Count(p2 => CanResolve(p2.ParameterType)))
                    .FirstOrDefault();

                if (ctor == null)
                {
                    throw new AdoException(string.Format("Unable to resolve type {0}: No constructor found for which all parameters can be resolved",t));
                }

                var pars = (from p in ctor.GetParameters() 
                            let tp = p.ParameterType 
                            let dv = p.HasDefaultValue ? p.DefaultValue : null 
                            select CanResolve(tp) ? new Func<object>(() => Resolve(tp)) : () => dv).ToArray();

                if (!singleton)
                {
                    return ctor.GetParameters().Length == 0
                        ? new Func<object>(() => Activator.CreateInstance(implType))
                        : () => ctor.Invoke(pars.Select(p => p()).ToArray());
                }

                // singleton...
                object instance = null;
                return ctor.GetParameters().Length == 0
                    ? new Func<object>( () => instance ?? (instance = Activator.CreateInstance(implType)))
                    : () =>  instance ?? (instance = ctor.Invoke(pars.Select(p => p()).ToArray()));
            });
        }

        private void EnsureNotReadonly()
        {
            if (_readOnly)
            {
                throw new AdoException("You cannot bind additional (nor replacing) service types after you have resolved any service type. So make sure you bind all needed types ASAP, before any service type could have been resolved. Always put your bind statements on top inside your initializer.");
            }
        }
        #endregion

    }

    public partial class Extensions
    {
        public static IAdoClassBinder Bind<TService>(this IAdoClassBinder self, Func<IAdoClassBinder, TService> factory)
        {
            return self.Bind(typeof(TService), b => factory(b));
        }

        public static IAdoClassBinder Bind<TService, TImpl>(this IAdoClassBinder self)
            where TImpl : TService
        {
            return self.Bind(typeof(TService), typeof(TImpl));
        }

        public static IAdoClassBinder BindSingleton<TService, TImpl>(this IAdoClassBinder self) where TImpl : TService
        {
            return self.BindSingleton(typeof(TService), typeof(TImpl));
        }

        public static bool CanResolve<TService>(this IAdoClassBinder self)
        {
            return self.CanResolve(typeof(TService));
        }

        public static T Get<T>(this IAdoClassBinder self)
        {
            return (T)self.Get(typeof(T));
        }
    }
}
