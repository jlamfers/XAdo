using System.Data;
using XAdo.Core;
using XAdo.Core.Interface;

namespace XAdo
{
    public static class AdoConnection
    {
        private static IAdoConnectionFactory ConnectionFactory
        {
            get { return AdoContext.Default.GetInstance<IAdoConnectionFactory>(); }
        }

        public static IDbConnection CreateConnection(string connectionStringName, bool keepConnectionAlive = false)
        {
            return ConnectionFactory.CreateConnection(connectionStringName, keepConnectionAlive);
        }

        public static IDbConnection CreateConnection(string connectionStringName, string providerName, bool keepConnectionAlive = false)
        {
            return ConnectionFactory.CreateConnection(connectionStringName, providerName, keepConnectionAlive);
        }
    }
}
