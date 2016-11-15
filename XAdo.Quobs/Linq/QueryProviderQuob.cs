using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.Linq
{
   internal class QueryProviderQuob : IQueryProvider
   {
      private readonly IQuob _quob;

      public QueryProviderQuob(IQuob quob)
      {
         _quob = quob;
      }

      public IQueryable CreateQuery(Expression expression)
      {
         var elementType = expression.Type.GetGenericArguments()[0];
         try
         {
            return (IQueryable)Activator.CreateInstance(typeof(QueryableQuob<>).MakeGenericType(elementType), new object[] { this, expression });
         }
         catch (TargetInvocationException ex)
         {
            throw ex.InnerException;
         }
      }

      // Queryable's collection-returning standard query operators call this method. 
      public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
      {
         return new QueryableQuob<TResult>(this, expression);
      }

      public object Execute(Expression expression)
      {
         var visitor = new QuobExpressionVisitor(_quob.CastTo<ICloneable>().Clone().CastTo<IQuob>());
        return visitor.Traverse(expression) ?? visitor.Quob.ToEnumerable();
      }

      // Queryable's "single value" standard query operators call this method.
      // It is also called from YepQueryable.GetEnumerator(). 
      public TResult Execute<TResult>(Expression expression)
      {
         var visitor = new QuobExpressionVisitor(_quob.CastTo<ICloneable>().Clone().CastTo<IQuob>());
         return  (TResult)(visitor.Traverse(expression) ?? visitor.Quob.ToEnumerable());
      }

   }
}