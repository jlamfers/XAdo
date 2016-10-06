using System;
using System.Collections.Generic;

namespace XAdo.Schema
{
   [Serializable]
   public class DbItem
   {
      [NonSerialized]
      private DbDatabase _database;

      private readonly IDictionary<string, object> _items = new Dictionary<string, object>();

      public DbDatabase Database
      {
         get { return _database; }
         internal set { _database = value; }
      }

      public IDictionary<string, object> Items
      {
         get { return _items; }
      }
   }
}