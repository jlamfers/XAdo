using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Core
{
    public class AnonymousTypeHelper : DtoTypeBuilder
    {
        private class Key
        {
            private IList<string> _names;
            private IList<Type> _types;
            private readonly int _hashcode;

            public Key(IList<string> names, IList<Type> types)
            {
                _names = names;
                _types = types;
                const int prime = 2791;
                var hashcode = prime;
                unchecked
                {
                    foreach (var n in names)
                    {
                        hashcode = hashcode * prime + n.GetHashCode();
                    }
                    foreach (var t in types)
                    {
                        hashcode = hashcode * prime + t.GetHashCode();
                    }
                }
                _hashcode = hashcode;
            }

            public override int GetHashCode()
            {
                return _hashcode;
            }

            public override bool Equals(object obj)
            {
                var other = obj as Key;
                return other != null &&  other._names.SequenceEqual(_names) && other._types.SequenceEqual(_types);
            }

            public void LazyCloneArrays()
            {
                _names = _names.ToArray();
                _types = _types.ToArray();
            }
        }

        private static readonly ConcurrentDictionary<object,Type> 
            _cache = new ConcurrentDictionary<object, Type>();


        public static Type GetOrCreateType(IList<string> names, IList<Type> types, string typeName = null)
        {
            return _cache.GetOrAdd((object)typeName ?? new Key(names, types), k =>
            {
                // cloning the inner arrays is needed to ensure not to get effected by outer changes, only if the particular key instance is stored into the cache
                ((Key)k).LazyCloneArrays();

                var b = new AnonymousTypeHelper { TypeName = typeName };
                for (var i = 0; i < names.Count; i++)
                {
                    b.ImplementProperty(names[i], types[i]);
                }
                return b.CreateType();
            });
        }

        protected override string GetUniqueName()
        {
            return "<anonymous_" + NextUniqueNumber() + ">";
        }

    }
}