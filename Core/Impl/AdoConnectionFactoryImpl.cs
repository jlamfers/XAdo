using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoConnectionFactoryImpl : IAdoConnectionFactory
    {
        public virtual IDbConnection CreateConnection(string connectionStringName, bool openConnection = false)
        {
            if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");

            try
            {
                var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
                return CreateConnection(cs.ConnectionString, DbProviderFactories.GetFactory(cs.ProviderName), openConnection);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("Unable to create connection from connection string name: " + connectionStringName, ex);
            }
        }

        public virtual IDbConnection CreateConnection(string connectionString, string providerName, bool openConnection = false)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (providerName == null) throw new ArgumentNullException("providerName");

            try
            {
                return CreateConnection(connectionString, DbProviderFactories.GetFactory(providerName), openConnection);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("Unable to create connection from provider name : " + providerName +" and connection string: " + connectionString, ex);
            }
        }

        private IDbConnection CreateConnection(string connectionString, DbProviderFactory factory, bool openConnection = false)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (factory == null) throw new ArgumentNullException("factory");

            try
            {
                var cn = factory.CreateConnection();
                if (cn == null)
                {
                    throw new AdoException("Unable to create connection: CreateConnection() returned null.");
                }
                cn.ConnectionString = connectionString;
                if (openConnection)
                {
                    cn.Open();
                }
                return cn;
            }
            catch (Exception ex)
            {
                throw new AdoException("Unable to create connection from connection string: " + connectionString + " and provider factory: " + factory.GetType(), ex);
            }
        }
    }
}
