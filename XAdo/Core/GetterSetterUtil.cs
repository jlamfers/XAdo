using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using XAdo.Core.Cache;

namespace XAdo.Core
{
    public static class GetterSetterUtil
    {
        public interface IGetterSetter
        {
            IGetterSetter Initialize(PropertyInfo propertyInfo);
            IGetterSetter Initialize(string name, object value, Type type = null);
            object Get(object entity);
            void Set(object entity, object value);
            bool CanRead { get; }
            bool CanWrite { get; }
            Type Type { get; }
            string Name { get; }
        }

        private class PropertyGetterSetter<TEntity, TValue> : IGetterSetter
        {
            private Func<TEntity, TValue> _getter;
            private Action<TEntity, TValue> _setter;
            private Type _type;
            private string _name;
            private PropertyInfo _propertyInfo;

            public IGetterSetter Initialize(PropertyInfo propertyInfo)
            {
                var getMethod = propertyInfo.GetGetMethod(true);
                _getter = getMethod != null ? (Func<TEntity, TValue>)Delegate.CreateDelegate(typeof(Func<TEntity, TValue>), getMethod) : null;
                var setMethod = propertyInfo.GetSetMethod(true);
                _setter = setMethod != null ? (Action<TEntity, TValue>)Delegate.CreateDelegate(typeof(Action<TEntity, TValue>), setMethod) : null;
                _type = propertyInfo.PropertyType;
                _name = propertyInfo.Name;
                _propertyInfo = propertyInfo;
                return this;
            }

            public IGetterSetter Initialize(string name, object value, Type type)
            {
                throw new NotImplementedException();
            }


            public object Get(object entity)
            {
                EnsureGetter();
                return _getter((TEntity)entity);
            }

            public void Set(object entity, object value)
            {
                EnsureSetter();
                _setter((TEntity)entity, (TValue)value);
            }

            public bool CanRead
            {
                get { return _getter != null; }
            }

            public bool CanWrite
            {
                get { return _setter != null; }
            }

            public Type Type
            {
                get { return _type; }
            }

            public string Name
            {
                get { return _name; }
            }

            private void EnsureGetter()
            {
                if (_getter == null) throw new AdoException(_propertyInfo + " has no getter");
            }
            private void EnsureSetter()
            {
                if (_setter == null) throw new AdoException(_propertyInfo + " has no setter");
            }
        }

        private class KeyValueGetterSetter : IGetterSetter
        {
            public IGetterSetter Initialize(PropertyInfo propertyInfo)
            {
                throw new NotImplementedException();
            }

            public IGetterSetter Initialize(string name, object value, Type type)
            {
                Name = name;
                Type = type ?? (value == null ? typeof (object) : value.GetType());
                return this;
            }

            public object Get(object entity)
            {
                var dict = (IDictionary<string, object>)entity;
                return dict[Name];
            }

            public void Set(object entity, object value)
            {
                var dict = (IDictionary<string, object>)entity;
                dict[Name] = value;
            }

            public bool CanRead
            {
                get { return true; }
            }

            public bool CanWrite
            {
                get { return true; }
            }

            public Type Type { get; private set; }

            public string Name { get; private set; }
        }

        private static readonly LRUCache<PropertyInfo, IGetterSetter>
            PropertyGetterSetterCache = new LRUCache<PropertyInfo, IGetterSetter>("LRUCache.XAdo.Properties.Size",2000);

        public static IGetterSetter ToGetterSetter(this PropertyInfo self)
        {
            return PropertyGetterSetterCache.GetOrAdd(self, p => (
                (IGetterSetter)Activator.CreateInstance(typeof (PropertyGetterSetter<,>).MakeGenericType(p.DeclaringType,p.PropertyType))).Initialize(self)
            );
        }

        public static IGetterSetter ToGetterSetter(this KeyValuePair<string,object> self)
        {
            return new KeyValueGetterSetter().Initialize(self.Key,self.Value,null);
        }

    }
}
