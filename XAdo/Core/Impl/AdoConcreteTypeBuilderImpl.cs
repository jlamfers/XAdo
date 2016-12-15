using System;
using System.Collections.Concurrent;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoConcreteTypeBuilderImpl : IAdoConcreteTypeBuilder
    {
        private readonly ConcurrentDictionary<Type, Type>
            _cache = new ConcurrentDictionary<Type, Type>();

        public virtual Type GetConcreteType(Type type)
        {
            if (!type.IsInterface)
            {
                return type;
            }

            return _cache.GetOrAdd(type, t =>
            {
                var builder = new DtoTypeBuilder();
                builder.ImplementInterface(type);
                return builder.CreateType();
            });
        }

    }
}
