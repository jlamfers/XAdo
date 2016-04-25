using System;
using System.Collections.Generic;

namespace XAdo.Core
{
    public interface IAnonymousTypeBuilder
    {
        Type GetAnonymousType(IList<string> propertyName, IList<Type> propertyTypes, string typeName = null);
    }
}