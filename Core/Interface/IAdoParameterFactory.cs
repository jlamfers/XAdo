using System;
using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoParameterFactory
    {
        IAdoParameter Create(object value = null, DbType? dbType = null, ParameterDirection? direction = null, byte? precision = null, byte? scale = null, int? size = null);
        IAdoParameterFactory SetCustomDefaultTypeMapping(Type parameterType, DbType dbType);
    }
}