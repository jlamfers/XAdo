using System;

namespace Sql.Parser.Core.Mapper
{
   [Flags]
   public enum PersistencyType
   {
      Default = 7, // all
      Create = 1,
      Read = 2,
      Update = 4,
      None = 0
   }
}