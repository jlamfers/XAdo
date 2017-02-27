using System.Collections.Generic;

namespace XAdo.Sql
{
   public class AsyncCountListResult<T>
   {
      public List<T> Collection { get; set; }
      public int TotalCount { get; set; }

   }
}