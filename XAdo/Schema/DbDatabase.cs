using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XAdo.Schema
{
   [Serializable]
   public class DbDatabase : IEnumerable<DbTable>, ISerializable
   {
      public DbDatabase(string name)
      {
         Name = name;
         Tables = new List<DbTable>();
         Columns = new List<DbColumn>();
         FKeys = new List<DbFKey>();
      }

      protected DbDatabase(SerializationInfo info, StreamingContext context)
      {
         Tables = (IList<DbTable>)info.GetValue("Tables", typeof(IList<DbTable>));
         Columns = (IList<DbColumn>)info.GetValue("Columns", typeof(IList<DbColumn>));
         FKeys = (IList<DbFKey>)info.GetValue("FKeys", typeof(IList<DbFKey>));

         foreach (var t in Tables) t.Database = this;
         foreach (var t in Columns) t.Database = this;
         foreach (var t in FKeys) t.Database = this;
      }

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("Tables",Tables);
         info.AddValue("Columns", Columns);
         info.AddValue("FKeys", FKeys);
      }


      public string Name { get; private set; }
      public IList<DbTable> Tables { get; private set; }
      public IList<DbColumn> Columns { get; private set; }
      public IList<DbFKey> FKeys { get; private set; }

      internal DbDatabase AsReadOnly()
      {
         var tables = Tables as List<DbTable>;
         var columns = Columns as List<DbColumn>;
         var fkeys = FKeys as List<DbFKey>;
         if (tables != null) Tables = tables.AsReadOnly();
         if (columns != null) Columns = columns.AsReadOnly();
         if (fkeys != null) FKeys = fkeys.AsReadOnly();
         return this;
      }

      public IEnumerator<DbTable> GetEnumerator()
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
