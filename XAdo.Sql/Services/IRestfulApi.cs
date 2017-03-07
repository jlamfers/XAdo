using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XAdo.Quobs.Services
{
   public interface IRestfulApi
   {
      object Put(object instance);
      IList<object> Get(string selectExpression, string filterExpression, string orderExpression, int? page, int? limit);
      bool Delete(string filterExpression);
      object Post(object instance);
      object Patch(object instance);
   }

   public interface IRestfulApi<T>
   {
      T Put(T instance);
      IList<T> Get(IList<Expression<Func<T, object>>> selectExpressions, Expression<Func<T, bool>> filterExpression, IList<Tuple<Expression<Func<T, object>>,bool>> orderExpressions, int? page, int? limit);
      IList<T> Get(string selectExpression, string filterExpression, string orderExpression, int? page, int? limit);
      bool Delete(string filterExpression);
      bool Delete(Expression<Func<T,bool>> expression);
      T Post(T instance);
      T Patch(object instance);
   }

}
