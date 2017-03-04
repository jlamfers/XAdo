using System;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class XAdoConcreteTypeBuilderImpl : IXAdoConcreteTypeBuilder
    {
        private readonly LRUCache<Type, Type>
            _cache = new LRUCache<Type, Type>("LRUCache.XAdo.Types.Size",2000);

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
