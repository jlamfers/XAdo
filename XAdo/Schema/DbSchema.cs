using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XAdo.Schema
{
   [Serializable]
   public class DbSchema : IEnumerable<TableSchemaItem>, ISerializable
   {
      public DbSchema(string name)
      {
         Name = name;
         Tables = new List<TableSchemaItem>();
         Columns = new List<ColumnSchemaItem>();
         FKeys = new List<FKeySchemaItem>();
      }

      protected DbSchema(SerializationInfo info, StreamingContext context)
      {
         Tables = (IList<TableSchemaItem>)info.GetValue("Tables", typeof(IList<TableSchemaItem>));
         Columns = (IList<ColumnSchemaItem>)info.GetValue("Columns", typeof(IList<ColumnSchemaItem>));
         FKeys = (IList<FKeySchemaItem>)info.GetValue("FKeys", typeof(IList<FKeySchemaItem>));

         foreach (var t in Tables) t.Schema = this;
         foreach (var t in Columns) t.Schema = this;
         foreach (var t in FKeys) t.Schema = this;
      }

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("Tables",Tables);
         info.AddValue("Columns", Columns);
         info.AddValue("FKeys", FKeys);
      }


      public string Name { get; private set; }
      public IList<TableSchemaItem> Tables { get; private set; }
      public IList<ColumnSchemaItem> Columns { get; private set; }
      public IList<FKeySchemaItem> FKeys { get; private set; }

      internal DbSchema AsReadOnly()
      {
         var tables = Tables as List<TableSchemaItem>;
         var columns = Columns as List<ColumnSchemaItem>;
         var fkeys = FKeys as List<FKeySchemaItem>;
         if (tables != null) Tables = tables.AsReadOnly();
         if (columns != null) Columns = columns.AsReadOnly();
         if (fkeys != null) FKeys = fkeys.AsReadOnly();
         return this;
      }

      public IEnumerator<TableSchemaItem> GetEnumerator()
      {
         return Tables.GetEnumerator();
      }

      public override string ToString()
      {
         return Name;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}
