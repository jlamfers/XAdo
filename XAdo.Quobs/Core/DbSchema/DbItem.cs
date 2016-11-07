using System;
using System.Collections.Generic;

namespace XAdo.Quobs.Core.DbSchema
{
   [Serializable]
   public class DbItem
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