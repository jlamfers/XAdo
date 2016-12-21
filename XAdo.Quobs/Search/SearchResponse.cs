using System.Collections.Generic;

namespace XAdo.SqlObjects.Search
{
   public class SearchResponse<TEntity>
   {
      public IList<TEntity> Collection { get; set; }
      public Paging Paging { get; set; }
      public int TotalCount { get; set; }

   }
}