using System;

namespace XAdo.Core.Interface
{
    public interface IXAdoConcreteTypeBuilder
    {
        Type GetConcreteType(Type type);
    }
}
