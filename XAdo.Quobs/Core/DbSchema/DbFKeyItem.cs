using System;

namespace XAdo.Quobs.Core.DbSchema
{
   [Serializable]
   public class DbFKeyItem : DbItem
   {
      public DbFKeyItem(DbSchema schema, string fKeyConstraintName, string fKeyTableSchema, string fKeyTableName, string fKeyColumnName, string refConstraintName, string refTableSchema, string refTableName, string refColumnName)
      {
         Schema = schema;

         FKeyConstraintName = fKeyConstraintName;
         FKeyTableSchema = fKeyTableSchema;
         FKeyTableName = fKeyTableName;
         FKeyColumnName = fKeyColumnName;

         RefConstraintName = refConstraintName;
         RefTableSchema = refTableSchema;
         RefTableName = refTableName;
         RefColumnName = refColumnName;
      }

      public string FKeyConstraintName { get; private set; }
      public string FKeyTableSchema { get; private set; }
      public string FKeyTableName { get; private set; }
      public string FKeyColumnName { get; private set; }
      public string RefConstraintName { get; private set; }
      public string RefTableSchema { get; private set; }
      public string RefTableName { get; private set; }
      public string RefColumnName { get; private set; }

      public override string ToString()
      {
         return string.Format("{0}.{1}.{2} => {3}.{4}.{5}", FKeyTableSchema, FKeyTableName, FKeyColumnName,
            RefTableSchema, RefTableName, RefColumnName);
      }
   }
}