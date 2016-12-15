using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XAdo.SqlObjects.DbSchema
{
   [Serializable]
   public class DbSchema : IEnumerable<DbTableItem>, ISerializable
   {
      public DbSchema(string name)
      {
         Name = name;
         Tables = new List<DbTableItem>();
         Columns = new List<DbColumnItem>();
         FKeys = new List<DbFKeyItem>();
      }

      protected DbSchema(SerializationInfo info, StreamingContext context)
      {
         Tables = (IList<DbTableItem>)info.GetValue("Tables", typeof(IList<DbTableItem>));
         Columns = (IList<DbColumnItem>)info.GetValue("Columns", typeof(IList<DbColumnItem>));
         FKeys = (IList<DbFKeyItem>)info.GetValue("FKeys", typeof(IList<DbFKeyItem>));

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
      public IList<DbTableItem> Tables { get; private set; }
      public IList<DbColumnItem> Columns { get; private set; }
      public IList<DbFKeyItem> FKeys { get; private set; }

      internal DbSchema AsReadOnly()
      {
         var tables = Tables as List<DbTableItem>;
         var columns = Columns as List<DbColumnItem>;
         var fkeys = FKeys as List<DbFKeyItem>;
         if (tables != null) Tables = tables.AsReadOnly();
         if (columns != null) Columns = columns.AsReadOnly();
         if (fkeys != null) FKeys = fkeys.AsReadOnly();
         return this;
      }

      public IEnumerator<DbTableItem> GetEnumerator()
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
