using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects
{
   public class SqlObjectFactory : ISqlObjectFactory
   {
      public virtual CreateSqlObject<TTable> CreateCreateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new CreateSqlObject<TTable>(connection);
      }

      public virtual QuerySqlObject<TTable> CreateReadSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new QuerySqlObject<TTable>(connection);
      }

      public virtual UpdateSqlObject<TTable> CreateUpdateSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new UpdateSqlObject<TTable>(connection);
      }

      public virtual DeleteSqlObject<TTable> CreateDeleteSqlObject<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new DeleteSqlObject<TTable>(connection);
      }

      public ITablePersister<TTable> CreateTablePersister<TTable>(ISqlConnection connection) where TTable : IDbTable
      {
         return new TablePersister<TTable>(connection);
      }
   }
}
