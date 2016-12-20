using System;
using System.Linq.Expressions;

namespace XHour.Contract.Search.Typed
{
   public class SearchRequest<TEntity>
   {
      public Expression<Func<TEntity, bool>> Predicate { get; set; }
      public Paging Paging { get; set; }
      public OrderByFieldExpressionList<TEntity> OrderByFields { get; set; }
   }
}
