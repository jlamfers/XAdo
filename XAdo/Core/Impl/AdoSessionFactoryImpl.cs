using System;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoSessionFactoryImpl : IAdoSessionFactory
    {
        private readonly IAdoClassBinder _classBinder;

        public AdoSessionFactoryImpl(IAdoClassBinder classBinder)
        {
            if (classBinder == null) throw new ArgumentNullException("classBinder");
            _classBinder = classBinder;
        }

        public virtual IAdoSession Create(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            return _classBinder.Get<IAdoSession>().CastTo<IAdoSessionInitializer>().Initialize(connectionStringName,commandTimeout,keepConnectionOpen,allowUnbindableFetchResults,allowUnbindableProperties);
        }

        public IAdoSession Create(string connectionString, string providerName, int? commandTimeout = null,
            bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false)
        {
            return _classBinder.Get<IAdoSession>().CastTo<IAdoSessionInitializer>().Initialize(connectionString, providerName, commandTimeout, keepConnectionOpen, allowUnbindableFetchResults, allowUnbindableProperties);
        }
    }
}
