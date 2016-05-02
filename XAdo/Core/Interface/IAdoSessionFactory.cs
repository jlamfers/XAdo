namespace XAdo.Core.Interface
{
    public interface IAdoSessionFactory
    {
        IAdoSession Create(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
        IAdoSession Create(string connectionString, string providerName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
    }
}
