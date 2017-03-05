namespace XAdo.Core.Interface
{
    /// <summary>
    /// This interface must as well be implemented by any IAdoSession implementation
    /// </summary>
    public interface IXAdoSessionInitializer
    {
        IXAdoDbSession Initialize(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
        IXAdoDbSession Initialize(string connectionString, string providerName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableMembers = false);
    }
}