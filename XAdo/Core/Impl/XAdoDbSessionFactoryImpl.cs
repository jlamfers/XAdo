using System;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class XAdoDbSessionFactoryImpl : IXAdoDbSessionFactory
    {
        private readonly IXAdoClassBinder _classBinder;

        public XAdoDbSessionFactoryImpl(IXAdoClassBinder classBinder)
        {
            if (classBinder == null) throw new ArgumentNullException("classBinder");
            _classBinder = classBinder;
        }

        public virtual IXAdoDbSession Create(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            return _classBinder.Get<IXAdoDbSession>().CastTo<IXAdoSessionInitializer>().Initialize(connectionStringName,commandTimeout,keepConnectionOpen,allowUnbindableFetchResults,allowUnbindableMembers);
        }

        public virtual IXAdoDbSession Create(string connectionString, string providerName, int? commandTimeout = null,
            bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            return _classBinder.Get<IXAdoDbSession>().CastTo<IXAdoSessionInitializer>().Initialize(connectionString, providerName, commandTimeout, keepConnectionOpen, allowUnbindableFetchResults, allowUnbindableMembers);
        }
    }
}
