using System;

namespace XAdo.Sql.Core.Mapper
{
   [Flags]
   public enum PersistencyType
   {
      None = 0,
      Default = 15, // all
      Create = 1,
      Read = 2,
      Update = 4,
      Delete = 8
   }
}