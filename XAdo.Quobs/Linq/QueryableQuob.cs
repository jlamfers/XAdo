using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;

namespace XAdo.Quobs.Linq
{
   public class QueryableQuob<TData> : IOrderedQueryable<TData>
   {
      public QueryableQuob(IQuob quob)
      {
         Provider = new QueryProviderQuob(quob);
         Expression = Expression.Constant(this);
      }

      public QueryableQuob(IQueryProvider provider, Expression expression)
      {
         if (provider == null)
         {
            throw new ArgumentNullException("provider");
         }

         if (expression == null)
         {
            throw new ArgumentNullException("expression");
         }

         if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
         {
            throw new ArgumentOutOfRangeException("expression", "expression has invalid type");
         }

         Provider = provider;
         Expression = expression;
      }


      public IQueryProvider Provider { get; private set; }

      public Expression Expression { get; private set; }

      public Type ElementType
      {
         get { return typeof(TData); }
      }

      public IEnumerator<TData> GetEnumerator()
      {
         return (Provider.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
      }
   }
}