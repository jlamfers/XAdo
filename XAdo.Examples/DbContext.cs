using XAdo.Core.Impl;

namespace XAdo.Examples
{
    public sealed partial class DbContext
    {
        public static readonly AdoContext
            Northwind = new AdoContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=AdventureWorks2012;integrated security=SSPI", "System.Data.SqlClient")
                .EnableFieldBinding()
                .KeepConnectionAlive(true)
                .AllowUnbindableProperties(false)
                .EnableEmittedDynamicTypes()
            ),

            NorthwindEmitted = new AdoContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=AdventureWorks2012;integrated security=SSPI", "System.Data.SqlClient")
                .EnableFieldBinding()
                .KeepConnectionAlive(true)
                .AllowUnbindableProperties(false)
            );
    }
}
