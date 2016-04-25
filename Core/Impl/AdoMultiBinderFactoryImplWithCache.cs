using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace XAdo.Core.Impl
{
    public class AdoMultiBinderFactoryImplWithCache : AdoMultiBinderFactoryImpl
    {
        private class Key
        {
            private readonly Type _t1;
            private readonly Type _tnext;
            private readonly bool _allowUnbindableFetchResults;
            private readonly bool _allowUnbindableProperties;
            private readonly int _nextIndex;
            private readonly int _fieldCount;
            private readonly int _hashcode;

            public Key(Type t1, Type tnext, bool allowUnbindableFetchResults, bool allowUnbindableProperties,int nextIndex, int fieldCount)
            {
                _t1 = t1;
                _tnext = tnext;
                _allowUnbindableFetchResults = allowUnbindableFetchResults;
                _allowUnbindableProperties = allowUnbindableProperties;
                _nextIndex = nextIndex;
                _fieldCount = fieldCount;
                const int prime = 31;
                unchecked
                {
                    _hashcode = prime;
                    _hashcode = _hashcode*prime + _allowUnbindableFetchResults.GetHashCode();
                    _hashcode = _hashcode*prime + _allowUnbindableProperties.GetHashCode();
                    _hashcode = _hashcode*prime + _nextIndex.GetHashCode();
                    _hashcode = _hashcode*prime + _fieldCount.GetHashCode();
                    _hashcode = _hashcode*prime + _t1.GetHashCode();
                    _hashcode = _hashcode*prime + _tnext.GetHashCode();
                }
            }

            public override int GetHashCode()
            {
                return _hashcode;
            }

            public override bool Equals(object obj)
            {
                var other = obj as Key;
                return other != null && other._allowUnbindableFetchResults == _allowUnbindableFetchResults
                       && other._allowUnbindableProperties == _allowUnbindableProperties
                       && other._fieldCount == _fieldCount
                       && other._nextIndex == _nextIndex
                       && other._t1 == _t1
                       && other._tnext == _tnext;
            }
        }

        private class Result
        {
            public object List;
            public int NextIndex;

        };

        private ConcurrentDictionary<object, object>
            _cache = new ConcurrentDictionary<object, object>();

        public AdoMultiBinderFactoryImplWithCache(IAdoDataBinderFactory binderFactory) : base(binderFactory)
        {
        }

        public override IList<IAdoPropertyBinder<T>> InitializePropertyBinders<T, TNext>(IDataRecord record,
            bool allowUnbindableFetchResults,
            bool allowUnbindableProperties, ref int nextIndex)
        {
            if (typeof(T) == typeof(TVoid)) return null;

            var index = nextIndex;
            var result =
                (Result) _cache.GetOrAdd(
                    new Key(typeof (T), typeof (TNext), allowUnbindableFetchResults, allowUnbindableProperties,
                        nextIndex,
                        record.FieldCount), k =>
                        {
                            var list = base.InitializePropertyBinders<T, TNext>(record, allowUnbindableFetchResults,
                                allowUnbindableProperties,
                                ref index);
                            return new Result
                            {
                                List = list,
                                NextIndex = index
                            };
                        }
                    );
            nextIndex = result.NextIndex;
            return (IList<IAdoPropertyBinder<T>>) result.List;
        }
    }
}