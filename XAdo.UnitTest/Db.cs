﻿using XAdo.Core.Impl;

namespace XAdo.UnitTest
{
    public sealed partial class Db
    {
        public static readonly AdoContext
            Northwind = new AdoContext(i => i
                .SetConnectionString(@"data source=.\SqlExpress;initial catalog=AdventureWorks2012;integrated security=SSPI", "System.Data.SqlClient")
                .EnableFieldBinding()
                //.AllowUnbindableProperties(true)
                .EnableEmittedDynamicTypes()
                //.SetCustomTypeConverter<int,long>(x => 0L + x)
                //.EnablePessimisticDataReader()
            );
    }
}
