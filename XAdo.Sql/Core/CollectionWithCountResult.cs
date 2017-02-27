using System.Collections.Generic;

namespace XAdo.Quobs.Core
{
   public class CollectionWithCountResult<T>
   {
      public List<T> Collection { get; set; }
      public int TotalCount { get; set; }

   }
}