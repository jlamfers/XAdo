using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XAdo.DbSchema
{
   [Serializable]
   public class DbItem
   {
      [NonSerialized]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private DbSchema _schema;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private readonly IDictionary<string, object> _items = new Dictionary<string, object>();

      public DbSchema Schema
      {
         get { return _schema; }
         internal set { _schema = value; }
      }

      public IDictionary<string, object> Items
      {
         get { return _items; }
      }

      public virtual string Id
      {
         get { return _schema.DatabaseName; }
      }
   }
}