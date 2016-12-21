using System.Collections.Generic;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public class AsyncPagedResult<T>
   {
      public List<T> Collection { get; set; }
      public int TotalCount { get; set; }

   }
}
