namespace XAdo.Core.Interface
{
    public interface IXAdoSessionFactory
    {
        IXAdoSession Create(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
        IXAdoSession Create(string connectionString, string providerName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
    }
}
