using System;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.Search.Typed
{
   public class SearchRequest<TEntity>
   {
      public Expression<Func<TEntity, bool>> Predicate { get; set; }
      public Paging Paging { get; set; }
      public OrderByFieldExpressionList<TEntity> OrderByFields { get; set; }
   }
}
