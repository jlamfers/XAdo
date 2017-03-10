using System;
using System.Diagnostics;
using System.Linq;

namespace XAdo.DbSchema
{
   [Serializable]
   public class DbColumnItem : DbItem
   {
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private static readonly DbColumnItem Empty = new DbColumnItem();
      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private DbTableItem _table;

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private DbColumnItem _references;

      private DbColumnItem() { }

      public DbColumnItem(DbSchema database, string tableName, string tableSchema, string name, Type type, bool isPkey, bool isAutoIncrement, bool isNullable, bool isUnique, object defaultValue, int maxLength)
      {
         Schema = database;
         TableOwner = tableSchema;
         TableName = tableName;
         IsAutoIncrement = isAutoIncrement;
         IsNullable = isNullable;
         IsUnique = isUnique;
         DefaultValue = defaultValue;
         MaxLength = maxLength == int.MaxValue ? 0 : maxLength;
         IsPkey = isPkey;
         Type = type;
         Name = name;
      }

      public DbTableItem Table
      {
         get { return _table ?? (_table = Schema.Tables.Single(t => t.Owner == TableOwner && t.Name == TableName)); }
      }

      public DbColumnItem References
      {
         get
         {
            if (_references != null)
            {
               return _references == Empty ? null : _references;
            }
            var fkey = Schema.FKeys.SingleOrDefault(fk => fk.FKeyTableName == TableName && fk.FKeyTableSchema == TableOwner && fk.FKeyColumnName == Name);
            if (fkey == null)
            {
               _references = Empty;
               return null;
            }
            return _references =
               Schema.Columns.Single(c => c.TableName == fkey.RefTableName && c.TableOwner == fkey.RefTableSchema && c.Name == fkey.RefColumnName);
         }
      }

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private DbFKeyItem _fkey;
      public DbFKeyItem FKey
      {
         get
         {
            return References == null ? null : (_fkey ?? (_fkey = Schema.FKeys.SingleOrDefault(k => k.FKeyTableSchema == TableOwner && k.FKeyTableName == TableName && k.FKeyColumnName == Name && k.RefTableSchema==References.TableOwner && k.RefTableName == References.TableName && k.RefColumnName == References.Name)));
         }
      }

      public string TableName { get; private set; }
      public string TableOwner { get; private set; }
      public string Name { get; private set; }
      public Type Type { get; private set; }
      public bool IsPkey { get; private set; }
      public bool IsAutoIncrement { get; private set; }
      public bool IsNullable { get; private set; }
      public bool IsUnique { get; private set; }
      public object DefaultValue { get; private set; }
      public int MaxLength { get; private set; }

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _id;
      public override string Id
      {
         get { return _id ?? (_id = (Table + "." + Name).ToLower()); }
      }

      public override int GetHashCode()
      {
         return Id.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         if (ReferenceEquals(obj, this)) return true;
         var other = obj as DbColumnItem;
         return other != null && other.Id == Id;
      }

      public override string ToString()
      {
         var text = Table + "." + Name;
         if (IsPkey)
         {
            text += "*";
         }
         if (References != null)
         {
            text += " => " + References.Table+"."+References.Name;
         }
         return text;
      }
   }
}