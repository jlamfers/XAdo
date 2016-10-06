using System;
using System.Linq;

namespace XAdo.Schema
{
   [Serializable]
   public class ColumnSchemaItem : SchemaItem
   {
      private static readonly ColumnSchemaItem Empty = new ColumnSchemaItem();
      [NonSerialized]
      private TableSchemaItem _table;

      [NonSerialized]
      private ColumnSchemaItem _references;

      private ColumnSchemaItem() { }

      public ColumnSchemaItem(DbSchema database, string tableName, string tableSchema, string name, Type type, bool isPkey, bool isDbGenerated, bool isNullable, bool isUnique, object defaultValue, int maxLength)
      {
         Schema = database;
         TableOwner = tableSchema;
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

      public TableSchemaItem Table
      {
         get { return _table ?? (_table = Schema.Tables.Single(t => t.Owner == TableOwner && t.Name == TableName)); }
      }

      public ColumnSchemaItem References
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

      public string TableName { get; private set; }
      public string TableOwner { get; private set; }
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