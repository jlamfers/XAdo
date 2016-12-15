using System;

namespace XAdo.Core.Interface
{
    public interface IAdoConcreteTypeBuilder
    {
        Type GetConcreteType(Type type);
    }
}
