using System;

namespace XAdo.Core.Interface
{
    public interface IConcreteTypeBuilder
    {
        Type GetConcreteType(Type type);
    }
}
