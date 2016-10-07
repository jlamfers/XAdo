using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.Schema
{
   [Serializable]
   public class TableSchemaItem : SchemaItem
   {
      [NonSerialized]
      private IList<ColumnSchemaItem> _columns;

      [NonSerialized]
      private IList<TableSchemaItem> _fkeyTables;

      [NonSerialized]
      private IList<ColumnSchemaItem> _fkeyColumns;

      public TableSchemaItem(DbSchema schema, string owner, string name, bool isView)
      {
         Schema = schema;
         IsView = isView;
         Name = name;
         Owner = owner;
      }

      public string Owner { get; private set; }
      public string Name { get; private set; }
      public bool IsView { get; private set; }

      public IList<ColumnSchemaItem> Columns
      {
         get { return _columns ?? (_columns = Schema.Columns.Where(c => c.TableOwner == Owner && c.TableName == Name).ToList().AsReadOnly()); }
      }
      public IList<ColumnSchemaItem> FKeyColumns
      {
         get { return _fkeyColumns ?? (_fkeyColumns = Columns.Where(c => c.References != null).ToList().AsReadOnly()); }
      }
      public IList<TableSchemaItem> FKeyTables
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