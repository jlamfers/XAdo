using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.DbSchema
{
   [Serializable]
   public class DbTableItem : DbItem
   {
      [NonSerialized]
      private IList<DbColumnItem> _columns;

      [NonSerialized]
      private IList<DbTableItem> _fkeyTables;

      [NonSerialized]
      private IList<DbColumnItem> _fkeyColumns;

      public DbTableItem(DbSchema schema, string owner, string name, bool isView)
      {
         Schema = schema;
         IsView = isView;
         Name = name;
         Owner = owner;
      }

      public string Owner { get; private set; }
      public string Name { get; private set; }
      public bool IsView { get; private set; }

      public IList<DbColumnItem> Columns
      {
         get { return _columns ?? (_columns = Schema.Columns.Where(c => c.TableOwner == Owner && c.TableName == Name).ToList().AsReadOnly()); }
      }
      public IList<DbColumnItem> FKeyColumns
      {
         get { return _fkeyColumns ?? (_fkeyColumns = Columns.Where(c => c.References != null).ToList().AsReadOnly()); }
      }
      public IList<DbTableItem> FKeyTables
      {
         get
         {
            var self = this;
            return _fkeyTables ?? (_fkeyTables = Schema.Tables.Where(t => t.Columns.Any(c => c.References != null && c.References.Table == self)).ToList().AsReadOnly());
         }
      }

      public override string ToString()
      {
         return Schema+"."+Name;
      }
   }
}