using XAdo.Core.Impl;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Dialects.SqlServer2012;
using XAdo.Quobs.SqlObjects;

namespace XAdo.Quobs.UnitTests
{
    public sealed partial class Db
    {
        public static readonly AdoContext
            Northwind = new SqlObjectsContext(i => i
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
            Users = new SqlObjectsContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=Users;integrated security=SSPI", "System.Data.SqlClient")
                .SetSqlFormatter(new SqlServer2012Formatter())
            );
    }
}
