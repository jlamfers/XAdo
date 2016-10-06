using System;
using System.Linq;

namespace XAdo.Schema
{
   [Serializable]
   public class DbColumn : DbItem
   {
      private static readonly DbColumn Empty = new DbColumn();
      [NonSerialized]
      private DbTable _table;

      [NonSerialized]
      private DbColumn _references;

      private DbColumn() { }

      public DbColumn(DbDatabase database, string tableName, string tableSchema, string name, Type type, bool isPkey, bool isDbGenerated, bool isNullable, bool isUnique, object defaultValue, int maxLength)
      {
         Database = database;
         TableSchema = tableSchema;
         TableName = tableName;
         IsDbGenerated = isDbGenerated;
         IsNullable = isNullable;
         IsUnique = isUnique;
         DefaultValue = defaultValue;
         MaxLength = maxLength;
         IsPkey = isPkey;
         Type = type;
         Name = name;
      }

      public DbTable Table
      {
         get { return _table ?? (_table = Database.Tables.Single(t => t.Schema == TableSchema && t.Name == TableName)); }
      }

      public DbColumn References
      {
         get
         {
            if (_references != null)
            {
               return _references == Empty ? null : _references;
            }
            var fkey = Database.FKeys.SingleOrDefault(fk => fk.FKeyTableName == TableName && fk.FKeyTableSchema == TableSchema && fk.FKeyColumnName == Name);
            if (fkey == null)
            {
               _references = Empty;
               return null;
            }
            return _references =
               Database.Columns.Single(c => c.TableName == fkey.RefTableName && c.TableSchema == fkey.RefTableSchema && c.Name == fkey.RefColumnName);
         }
      }

      public string TableName { get; private set; }
      public string TableSchema { get; private set; }
      public string Name { get; private set; }
      public Type Type { get; private set; }
      public bool IsPkey { get; private set; }
      public bool IsDbGenerated { get; private set; }
      public bool IsNullable { get; private set; }
      public bool IsUnique { get; private set; }
      public object DefaultValue { get; private set; }
      public int MaxLength { get; private set; }

      private string _toString;
      public override string ToString()
      {
         if (_toString == null)
         {
            _toString = Table+"."+Name;
            if (References != null)
            {
               _toString += " => " + References.Table+"."+References.Name;
            }
         }
         return _toString;
      }
   }
}