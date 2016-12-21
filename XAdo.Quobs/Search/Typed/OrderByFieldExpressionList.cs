using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.Search.Typed
{
   public class OrderByFieldExpressionList<TEntity> : IEnumerable<OrderByFieldExpression>
   {
      private readonly List<OrderByFieldExpression>
         _orderByExpressions = new List<OrderByFieldExpression>();

      public OrderByFieldExpressionList<TEntity> Add<T>(Expression<Func<TEntity, T>> column, bool descending = false)
      {
         _orderByExpressions.Add(new OrderByFieldExpression{Field = column, Descending = descending});
         return this;
      }
      public OrderByFieldExpressionList<TEntity> Add(Expression column, bool descending = false)
      {
         _orderByExpressions.Add(new OrderByFieldExpression { Field = column, Descending = descending });
         return this;
      }

      public IEnumerator<OrderByFieldExpression> GetEnumerator()
      {
         return _orderByExpressions.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}