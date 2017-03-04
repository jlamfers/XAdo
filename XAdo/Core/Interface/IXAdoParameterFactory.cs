using System;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IXAdoParameterFactory
    {
        IXAdoParameter Create(object value = null, DbType? dbType = null, ParameterDirection? direction = null, byte? precision = null, byte? scale = null, int? size = null);
        IXAdoParameterFactory SetCustomDefaultTypeMapping(Type parameterType, DbType dbType);
        DbType? GetCustomDefaultTypeMapping(Type parameterType);
    }
}