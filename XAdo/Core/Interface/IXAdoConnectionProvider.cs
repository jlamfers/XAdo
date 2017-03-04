using System.Data;

namespace XAdo.Core.Interface
{
    public interface IXAdoConnectionProvider
    {
        IDbConnection Connection { get; }
       IDbTransaction Transaction { get; }
    }
}
