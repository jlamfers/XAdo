using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoConnectionFactory
    {
        IDbConnection CreateConnection(string connectionStringName, bool openConnection = false);
        IDbConnection CreateConnection(string connectionString, string providerName, bool openConnection = false);
    }
}