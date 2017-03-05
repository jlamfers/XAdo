namespace XAdo.Core.Interface
{
    public interface IXAdoDbSessionFactory
    {
        IXAdoDbSession Create(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
        IXAdoDbSession Create(string connectionString, string providerName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
    }
}
