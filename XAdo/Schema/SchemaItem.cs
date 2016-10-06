using System;
using System.Collections.Generic;

namespace XAdo.Schema
{
   [Serializable]
   public class SchemaItem
   {
      [NonSerialized]
      private DbSchema _schema;

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
   }
}