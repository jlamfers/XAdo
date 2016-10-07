using System;

namespace XAdo.Quobs.Schema
{
   [Serializable]
   public class FKeySchemaItem : SchemaItem
   {
      public FKeySchemaItem(DbSchema schema, string fKeyConstrantName, string fKeyTableSchema, string fKeyTableName, string fKeyColumnName, string refConstraintName, string refTableSchema, string refTableName, string refColumnName)
      {
         Schema = schema;

         FKeyConstrantName = fKeyConstrantName;
         FKeyTableSchema = fKeyTableSchema;
         FKeyTableName = fKeyTableName;
         FKeyColumnName = fKeyColumnName;

         RefConstraintName = refConstraintName;
         RefTableSchema = refTableSchema;
         RefTableName = refTableName;
         RefColumnName = refColumnName;
      }

      public string FKeyConstrantName { get; private set; }
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