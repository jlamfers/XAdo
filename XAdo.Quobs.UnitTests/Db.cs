using XAdo.Core.Impl;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs.UnitTests
{
    public sealed partial class Db
    {
        public static readonly AdoContext
            Northwind = new AdoContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=AdventureWorks2012;integrated security=SSPI", "System.Data.SqlClient")
                .EnableFieldBinding()
                .KeepConnectionAlive(true)
                //.AllowUnbindableProperties(true)
                .EnableEmittedDynamicTypes()
                //.EnablePessimisticDataReader()
                .SetCustomTypeConverter<int,long>(x => 0L + x)
                .SetSqlFormatter(new SqlServer2012Formatter())
            );

        public static readonly AdoContext
            Users = new AdoContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=Users;integrated security=SSPI", "System.Data.SqlClient")
                .SetSqlFormatter(new SqlServer2012Formatter())
            );
    }
}
