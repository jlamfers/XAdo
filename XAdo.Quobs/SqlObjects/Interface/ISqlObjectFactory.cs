using XAdo.Quobs.Core;
using XAdo.Quobs.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlObjectFactory
   {
      IWriteFromSqlObject<TTable>  CreateCreateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      ITableSqlObject<TTable> CreateReadSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      IWriteWhereSqlObject<TTable> CreateUpdateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      IWriteWhereSqlObject<TTable> CreateDeleteSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable;
      ITablePersister<TTable> CreateTablePersister<TTable>(ISqlConnection connection) where TTable : IDbTable;
   }
}
