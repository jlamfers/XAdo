namespace XAdo.Core.Interface
{
    /// <summary>
    /// This interface must as well be implemented by any IAdoSession implementation
    /// </summary>
    public interface IAdoSessionInitializer
    {
        IAdoSession Initialize(string connectionStringName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
        IAdoSession Initialize(string connectionString, string providerName, int? commandTimeout = null, bool keepConnectionOpen = false, bool allowUnbindableFetchResults = true, bool allowUnbindableProperties = false);
    }
}