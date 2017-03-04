using System.Data;

namespace XAdo.Core.Interface
{
    public interface IXAdoConnectionFactory
    {
        IDbConnection CreateConnection(string connectionStringName, bool openConnection = false);
        IDbConnection CreateConnection(string connectionString, string providerName, bool openConnection = false);
    }
}