using XAdo.SqlObjects.DbSchema;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public interface ISqlObjectFactory
   {
      CreateSqlObject<TTable>  CreateCreateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      QuerySqlObject<TTable> CreateReadSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      UpdateSqlObject<TTable> CreateUpdateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      DeleteSqlObject<TTable> CreateDeleteSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      ITablePersister<TTable> CreateTablePersister<TTable>(ISqlConnection connection) where TTable : IDbTable;
   }
}
