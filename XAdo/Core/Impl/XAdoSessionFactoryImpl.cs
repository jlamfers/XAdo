using System;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class XAdoSessionFactoryImpl : IXAdoSessionFactory
    {
        private readonly IXAdoClassBinder _classBinder;

        public XAdoSessionFactoryImpl(IXAdoClassBinder classBinder)
        {
            if (classBinder == null) throw new ArgumentNullException("classBinder");
            _classBinder = classBinder;
        }

        public virtual IXAdoSession Create(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            return _classBinder.Get<IXAdoSession>().CastTo<IXAdoSessionInitializer>().Initialize(connectionStringName,commandTimeout,keepConnectionOpen,allowUnbindableFetchResults,allowUnbindableMembers);
        }

        public virtual IXAdoSession Create(string connectionString, string providerName, int? commandTimeout = null,
            bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false)
        {
            return _classBinder.Get<IXAdoSession>().CastTo<IXAdoSessionInitializer>().Initialize(connectionString, providerName, commandTimeout, keepConnectionOpen, allowUnbindableFetchResults, allowUnbindableMembers);
        }
    }
}
