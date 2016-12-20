using System.Collections.Generic;

namespace XHour.Contract.Search
{
   public class SearchResponse<TEntity>
   {
      public IList<TEntity> Collection { get; set; }
      public Paging Paging { get; set; }
      public int TotalCount { get; set; }

   }
}