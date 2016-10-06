using System;

namespace XAdo.Schema
{
   [Serializable]
   public class DbFKey : DbItem
   {
      public DbFKey(DbDatabase database, string fKeyTableSchema, string fKeyTableName, string refTableSchema, string refTableName, string fKeyColumnName, string refColumnName)
      {
         Database = database;
         FKeyTableSchema = fKeyTableSchema;
         FKeyTableName = fKeyTableName;
         RefTableSchema = refTableSchema;
         RefTableName = refTableName;
         FKeyColumnName = fKeyColumnName;
         RefColumnName = refColumnName;
      }

      public string FKeyTableSchema { get; private set; }
      public string FKeyTableName { get; private set; }
      public string RefTableSchema { get; private set; }
      public string RefTableName { get; private set; }
      public string FKeyColumnName { get; private set; }
      public string RefColumnName { get; private set; }

      public override string ToString()
      {
         return string.Format("{0}.{1}.{2} => {3}.{4}.{5}", FKeyTableSchema, FKeyTableName, FKeyColumnName,
            RefTableSchema, RefTableName, RefColumnName);
      }
   }
}