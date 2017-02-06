using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects.SubQuery
{
   public interface ISubQuery
   {
      QuerySqlObject<TTable> From<TTable>() where TTable : IDbTable;
   }
}