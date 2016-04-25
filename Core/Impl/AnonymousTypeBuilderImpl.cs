using System;
using System.Collections.Generic;

namespace XAdo.Core.Impl
{
    public class AnonymousTypeBuilderImpl : IAnonymousTypeBuilder
    {
        public virtual Type GetAnonymousType(IList<string> propertyName, IList<Type> propertyTypes, string typeName = null)
        {
            return null;
        }
    }
}