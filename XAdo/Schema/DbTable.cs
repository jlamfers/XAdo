using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Schema
{
   [Serializable]
   public class DbTable : DbItem
   {
      [NonSerialized]
      private IList<DbColumn> _columns;

      [NonSerialized]
      private IList<DbTable> _fkeyTables;

      [NonSerialized]
      private IList<DbColumn> _fkeyColumns;

      public DbTable(DbDatabase database, string schema, string name, bool isView)
      {
         Database = database;
         IsView = isView;
         Name = name;
         Schema = schema;
      }

      public string Schema { get; private set; }
      public string Name { get; private set; }
      public bool IsView { get; private set; }

      public IList<DbColumn> Columns
      {
         get { return _columns ?? (_columns = Database.Columns.Where(c => c.TableSchema == Schema && c.TableName == Name).ToList().AsReadOnly()); }
      }
      public IList<DbColumn> FKeyColumns
      {
         get { return _fkeyColumns ?? (_fkeyColumns = Columns.Where(c => c.References != null).ToList().AsReadOnly()); }
      }
      public IList<DbTable> FKeyTables
      {
         get
         {
            var self = this;
            return _fkeyTables ?? (_fkeyTables = Database.Tables.Where(t => t.Columns.Any(c => c.References != null && c.References.Table == self)).ToList().AsReadOnly());
         }
      }

      public override string ToString()
      {
         return Schema+"."+Name;
      }
   }
}