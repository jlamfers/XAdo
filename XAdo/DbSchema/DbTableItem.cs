using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XAdo.DbSchema
{
   [Serializable]
   public class DbTableItem : DbItem
   {
      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private IList<DbColumnItem> _columns;

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private IList<DbTableItem> _childTables;

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private IList<DbColumnItem> _fkeyColumns;

      public DbTableItem(DbSchema schema, string owner, string name, string database, bool isView)
      {
         Schema = schema;
         IsView = isView;
         Name = name;
         Owner = owner;
         Database = database;
      }

      public string Database { get; private set; }
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
      public IList<DbTableItem> ChildTables
      {
         get
         {
            var self = this;
            return _childTables ?? (_childTables = Schema.Tables.Where(t => t.Columns.Any(c => c.References != null && c.References.Table.Equals(self))).ToList().AsReadOnly());
         }
      }

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _id;
      public override string Id
      {
         get { return _id ?? (_id = (Owner + "." + Name).Trim('.').ToLower()); }
      }

      public override int GetHashCode()
      {
         return Id.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         if (ReferenceEquals(obj, this)) return true;
         var other = obj as DbTableItem;
         return other != null && other.Id == Id;
      }

      public override string ToString()
      {
         return Owner + "." + Name;
      }
   }
}