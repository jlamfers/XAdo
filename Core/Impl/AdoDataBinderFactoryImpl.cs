using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{

    public class AdoDataBinderFactoryImpl : IAdoDataBinderFactory
    {
        private readonly IAdoTypeConverterFactory _typeConverterFactory;

        private readonly ConcurrentDictionary<BinderIdentity, object>
            _binderCache = new ConcurrentDictionary<BinderIdentity, object>();

        private static readonly HashSet<Type> NonPrimitiveBindableTypes = new HashSet<Type>(new[]
        {
            typeof (String),
            typeof (Decimal),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (TimeSpan),
            typeof (Guid), 
            typeof (byte[])
        });

        public AdoDataBinderFactoryImpl(IAdoTypeConverterFactory typeConverterFactory)
        {
            _typeConverterFactory = typeConverterFactory;
        }

        #region Types

        // copies value from datarecord to entity
        private class AdoPropertyBinder<TEntity, TSetter, TGetter> : IAdoPropertyBinder<TEntity>
        {
            private Action<TEntity, TSetter> _setter;
            private Func<IDataRecord, int, TSetter> _getter;
            private int _index;

            public AdoPropertyBinder()
            {
                _getter = GetterDelegate<TSetter>.Getter;
            }

            public IAdoPropertyBinder<TEntity> Initialize(PropertyInfo property, int index, IAdoTypeConverterFactory typeConverterFactory)
            {
                var setMethod = property.GetSetMethod(true);
                if (setMethod == null)
                {
                    throw new AdoException("No setter available for property " + property);
                }
                _setter =
                    (Action<TEntity, TSetter>)
                        Delegate.CreateDelegate(typeof(Action<TEntity, TSetter>), setMethod);
                if (!typeof (TSetter).IsAssignableFrom(typeof (TGetter)))
                {
                    var converter = typeConverterFactory.GetConverter<TSetter>(typeof(TGetter));
                    if (typeof (TSetter).IsValueType && Nullable.GetUnderlyingType(typeof (TSetter)) == null)
                    {
                        _getter = (d, i) => converter((TGetter) d.GetValue(i));
                    }
                    else
                    {
                        _getter = (d, i) => d.IsDBNull(i) ? default(TSetter) : converter((TGetter) d.GetValue(i));
                    }
                }
                _index = index;
                return this;
            }

            public void CopyValue(IDataRecord reader, TEntity entity)
            {
                _setter(entity, _getter(reader, _index));
            }

            public int Index
            {
                get { return _index; }
            }
        }

        // identity (key) for datarecord/type based property binders list
        private class BinderIdentity
        {
            private readonly Type _type;
            private readonly bool _allowUnbindableFetchResults;
            private readonly bool _allowUnbindableProperties;
            private readonly int? _firstColumnIndex;
            private readonly int? _lastColumnIndex;
            private readonly string[] _columnNames;
            private readonly Type[] _columnTypes;
            private readonly int _hash;

            public BinderIdentity(Type type, IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableProperties,
                int? firstColumnIndex, int? lastColumnIndex)
            {
                _type = type;
                _allowUnbindableFetchResults = allowUnbindableFetchResults;
                _allowUnbindableProperties = allowUnbindableProperties;
                _firstColumnIndex = firstColumnIndex;
                _lastColumnIndex = lastColumnIndex;
                _columnNames = new string[record.FieldCount];
                _columnTypes = new Type[record.FieldCount];
                unchecked
                {
                    const int factor = 1699;
                    _hash = _type.GetHashCode();
                    for (var i = 0; i < record.FieldCount; i++)
                    {
                        _columnNames[i] = record.GetName(i);
                        _columnTypes[i] = record.GetFieldType(i);
                        _hash = _hash * factor + _columnNames[i].GetHashCode();
                        _hash = _hash * factor + _columnTypes[i].GetHashCode();
                        if (allowUnbindableFetchResults) _hash = _hash * factor + 1;
                        if (allowUnbindableProperties) _hash = _hash * factor + 1;
                        if (firstColumnIndex != null) _hash = _hash * factor + firstColumnIndex.Value;
                        if (lastColumnIndex != null) _hash = _hash * factor + lastColumnIndex.Value;
                    }
                }
            }

            public override int GetHashCode()
            {
                return _hash;
            }

            public override bool Equals(object obj)
            {
                var other = (BinderIdentity)obj;
                return _type == other._type
                       && _allowUnbindableFetchResults == other._allowUnbindableFetchResults
                       && _allowUnbindableProperties == other._allowUnbindableProperties
                       && _firstColumnIndex == other._firstColumnIndex
                       && _lastColumnIndex == other._lastColumnIndex
                       && _columnNames.SequenceEqual(other._columnNames)
                       && _columnTypes.SequenceEqual(other._columnTypes);
            }
        }

        #endregion

        public virtual IAdoPropertyBinder<TEntity> CreatePropertyBinder<TEntity>(PropertyInfo property, Type getterType, int index)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (getterType == null) throw new ArgumentNullException("getterType");

            var binderGetterType = Nullable.GetUnderlyingType(property.PropertyType) == getterType
                ? property.PropertyType
                : getterType;
            return
                ((IAdoPropertyBinder<TEntity>)
                    Activator.CreateInstance(typeof (AdoPropertyBinder<,,>).MakeGenericType(typeof (TEntity),
                        property.PropertyType, binderGetterType)))
                    .Initialize(property, index,_typeConverterFactory);
        }

        public virtual Func<IDataReader, TResult> CreateScalarReader<TResult>(Type getterType)
        {
            if (getterType == null) throw new ArgumentNullException("getterType");

            if (typeof (TResult).IsAssignableFrom(getterType))
            {
                var getter = GetterDelegate<TResult>.Getter;
                if (getter != null)
                {
                    return r => getter(r, 0);
                }
            }
            var converter = _typeConverterFactory.GetConverter<TResult>(getterType);
            if (typeof (TResult).IsValueType && Nullable.GetUnderlyingType(typeof (TResult)) == null)
            {
                return r => converter(r.GetValue(0));
            }
            return r => r.IsDBNull(0) ? default(TResult) : converter(r.GetValue(0));
        }

        // initializes and caches a property binders list by entity type and a datareader structure
        // there is no risk of an out of memory issue
        public virtual IList<IAdoPropertyBinder<T>> CreatePropertyBinders<T>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableProperties, int? firstColumnIndex = null, int? lastColumnIndex = null)
        {
            if (record == null) throw new ArgumentNullException("record");
            return
                (IList<IAdoPropertyBinder<T>>)_binderCache.GetOrAdd(
                    new BinderIdentity(typeof(T), record, allowUnbindableFetchResults, allowUnbindableProperties, firstColumnIndex,lastColumnIndex),
                    k =>
                    {
                        var type = typeof (T);
                        var binders = new List<IAdoPropertyBinder<T>>();
                        var first = firstColumnIndex.GetValueOrDefault(0);
                        var last = lastColumnIndex.GetValueOrDefault(record.FieldCount - 1);
                        for (var i = first; i <= last; i++)
                        {
                            var p = type.GetProperty(record.GetName(i),BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (!allowUnbindableFetchResults && p == null)
                            {
                                throw new AdoBindingException("Cannot bind fetched column [" + record.GetName(i) + "] result to any property of type " + type.Name);
                            }
                            if (p != null)
                            {
                                binders.Add(CreatePropertyBinder<T>(p, record.GetFieldType(i), i));
                            }
                        }

                        if (binders.Count == 0)
                        {
                            if (record.FieldCount == 1)
                            {
                                throw new AdoBindingException("Cannot bind " + record.GetFieldType(0) + " to " + typeof (T));
                            }
                            throw new AdoBindingException("Type " + typeof (T) + " has no bindable properties");
                        }

                        if (!allowUnbindableProperties)
                        {
                            var set = new HashSet<string>();
                            for (var i = first; i <= last; i++)
                            {
                                set.Add(record.GetName(i));
                            }
                            if (GetBindableProperties(type).Any(p => !set.Contains(p.Name)))
                            {
                                throw new AdoBindingException("No bindable results for following " + type.Name +
                                                              "properties: " +
                                                              string.Join(", ",
                                                                  GetBindableProperties(type)
                                                                      .Where(p => !set.Contains(p.Name))
                                                                      .Select(p => p.Name)
                                                                      .ToArray()));
                            }
                        }

                        return binders.ToArray();
                    });
        }

        public virtual bool IsBindableDataType(Type type)
        {
            if (type == null) return false;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.IsPrimitive || type.IsEnum) return true;
            if (NonPrimitiveBindableTypes.Contains(type)) return true;

            // we do not need a reference to Microsoft.SqlServer.Types.SqlGeography here
            return type.FullName.StartsWith("Microsoft.SqlServer.Types.") && (
                   type.FullName.EndsWith(".SqlGeography")
                || type.FullName.EndsWith(".SqlGeometry")
                || type.FullName.EndsWith(".SqlHierarchyId")
            );
        }

        private IEnumerable<PropertyInfo> GetBindableProperties(Type type)
        {
            return type.GetProperties().Where(p => p.CanWrite && IsBindableDataType(p.PropertyType));
        }


    }

}


