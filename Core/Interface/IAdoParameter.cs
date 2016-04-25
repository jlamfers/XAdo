using System.Data;

namespace XAdo.Core.Interface
{
    public interface IAdoParameter
    {
        object Value { get; set; }
        DbType? DbType { get; set; }
        ParameterDirection? Direction { get; set; }
        byte? Precision { get; set; }
        byte? Scale { get; set; }
        int? Size { get; set; }
        IDbDataParameter CreateDbParameter(IDbCommand command, string name);
    }
}