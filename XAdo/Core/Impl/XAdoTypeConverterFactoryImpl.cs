using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class XAdoTypeConverterFactoryImpl : IXAdoTypeConverterFactory
    {

        private class ConverterKey
        {
            private readonly Type _sourceType;
            private readonly Type _targetType;
            private readonly int _hashcode;

            public ConverterKey(Type sourceType, Type targetType)
            {
                _sourceType = sourceType;
                _targetType = targetType;
                unchecked
                {
                    _hashcode = _sourceType.GetHashCode() * 17 + _targetType.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                var other = (ConverterKey)obj;
                return other._sourceType == _sourceType && other._targetType == _targetType;
            }

            public override int GetHashCode()
            {
                return _hashcode;
            }
        }

        private readonly ConcurrentDictionary<ConverterKey, Func<object, object>>
            _customConverters = new ConcurrentDictionary<ConverterKey, Func<object, object>>();

        private bool _readonly;


        public virtual IXAdoTypeConverterFactory SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> converter)
        {
            if (_readonly)
            {
                throw new XAdoException("You cannot set additional type converters after you have resolved any custom converter. So make sure bind all needed converters before using any of these.");
            }

            if (Nullable.GetUnderlyingType(typeof(TSource)) != null || Nullable.GetUnderlyingType(typeof(TTarget)) != null)
            {
                throw new XAdoException("You must not register nullable type converters. You do not need to. Custom nullable type converions are handled automatically if their corresponding value types have been registered for custom conversion.");
            }

            if (!_customConverters.TryAdd(new ConverterKey(typeof (TSource), typeof(TTarget)), (x => converter((TSource) x))))
            {
                throw new XAdoException("Converter already has been registered.");
            }

            return this;
        }

        public virtual bool CanCustomConvert(Type sourceType, Type targetType)
        {
            return _customConverters.ContainsKey(new ConverterKey(sourceType, targetType.EnsureNotNullable()));
        }

        public virtual Func<object, TTarget> GetConverter<TTarget>(Type sourceType)
        {
            _readonly = true;

            if (typeof(TTarget).IsAssignableFrom(sourceType))
            {
                // if types are the same, or assignable, return the simple casting delegate immediatly
                return x => (TTarget)x;
            }

            // now first check if we have any custom converters registered
            Func<object, object> customConverter;
            if (_customConverters.TryGetValue(new ConverterKey(sourceType, typeof(TTarget).EnsureNotNullable()), out customConverter))
            {
                // use custom converter
                return x => (TTarget)customConverter(x);
            }

            var setterType = typeof(TTarget).EnsureNotNullable();

            if (setterType.IsEnum && sourceType != typeof(string))
            {
                var enumUnderlyingType = Enum.GetUnderlyingType(setterType);
                if (sourceType != enumUnderlyingType)
                {
                    // e.g. convert byte to int first, before converting to enum
                    return x => (TTarget)Enum.ToObject(setterType, Convert.ChangeType(x, enumUnderlyingType));
                }
                return x => (TTarget)Enum.ToObject(setterType, x);
            }

            if (setterType == typeof(string))
            {
                var invariantCulture = CultureInfo.InvariantCulture;
                return x => (TTarget)(object)Convert.ToString(x, invariantCulture);
            }

            var converter = TypeDescriptor.GetConverter(setterType);

            // by default we follow the Microsoft conventions here
            // e.g. a cast from Int16 (from database) to Int32 (at entity) will fail.
            return x => (TTarget)converter.ConvertFrom(x);
        }
    }

}

