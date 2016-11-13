using XAdo.Core.Impl;
using XAdo.Quobs.Formatters;

namespace XAdo.Quobs.UnitTests
{
    public sealed partial class Db
    {
        public static readonly AdoContext
            Northwind = new AdoContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=AdventureWorks2012;integrated security=SSPI", "System.Data.SqlClient")
                .EnableFieldBinding()
                .KeepConnectionAlive(false)
                //.AllowUnbindableProperties(true)
                .EnableEmittedDynamicTypes()
                //.EnablePessimisticDataReader()
                .SetCustomTypeConverter<int,long>(x => 0L + x)
                .SetItem("quobs.sql.formatter",new Ms2012SqlFormatter())
            );
    }
}
