using System;

namespace XAdo.Sql.Core
{
   [Flags]
   public enum PersistencyType
   {
      Default=3, // all
      Create=1,
      Update=2,
      None=0
   }
}