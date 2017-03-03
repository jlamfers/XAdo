using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoConnectionProvider
    {
        IDbConnection Connection { get; }
       IDbTransaction Transaction { get; }
    }
}
