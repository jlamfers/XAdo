using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace XAdo.DbSchema
{
   [Serializable]
   public class DbSchema : IEnumerable<DbTableItem>, ISerializable
   {
      public DbSchema(string name)
      {
         DatabaseName = name;
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


      public string DatabaseName { get; private set; }
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
         return DatabaseName;
      }

      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IDictionary<string, DbItem> _lookup;
      public IDictionary<string, DbItem> LookupTable
      {
         get { return _lookup ?? (_lookup = new ReadOnlyDictionary<string, DbItem>(Tables.Cast<DbItem>().Concat(Columns).ToDictionary(x => x.Id, x => x))); }
      }

      public DbProviderInfo ProviderInfo { get; internal set; }

      public virtual DbColumnItem FindColumn(string key)
      {
         if (key == null) return null;
         DbItem item;
         LookupTable.TryGetValue(key.ToLower(), out item);
         return item as DbColumnItem;
      }
      public virtual DbColumnItem FindColumn(IEnumerable<string> key)
      {
         return key == null ? null : FindColumn(string.Join(".", key));
      }

      public virtual DbTableItem FindTable(string key)
      {
         if (key == null) return null;
         DbItem item;
         LookupTable.TryGetValue(key.ToLower(), out item);
         return item as DbTableItem;
      }
      public virtual DbTableItem FindTable(IEnumerable<string> key)
      {
         return key == null ? null : FindTable(string.Join(".", key));
      }


      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }

}
