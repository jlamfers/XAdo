using System.Data;

namespace XAdo.Core.Interface
{
    public interface IXAdoCommandFactory
    {
        IDbCommand CreateCommand(IDbConnection cn, string sql, object param, IDbTransaction tr, int? commandTimeout,CommandType? commandType);
        IDbCommand FillParams(IDbCommand command, object param);
        CommandType GetDefaultCommandType(string sql);
    }
}
