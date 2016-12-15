using XAdo.Quobs.Core;
using XAdo.Quobs.DbSchema.Attributes;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class SqlObjectFactory : ISqlObjectFactory
   {
      public virtual IWriteFromSqlObject<TTable> CreateCreateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new CreateSqlObject<TTable>(connection);
      }

      public virtual ITableSqlObject<TTable> CreateReadSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new TableSqlObject<TTable>(connection);
      }

      public virtual IWriteWhereSqlObject<TTable> CreateUpdateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new UpdateSqlObject<TTable>(connection);
      }

      public virtual IWriteWhereSqlObject<TTable> CreateDeleteSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new DeleteSqlObject<TTable>(connection);
      }

      public ITablePersister<TTable> CreateTablePersister<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new TablePersister<TTable>(connection);
      }
   }
}
