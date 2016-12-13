using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlMapObject<T> : ISqlFetchObject<T>
   {
      new ISqlMapObject<T> Distinct();
      ISqlMapObject<T> Where(Expression<Func<T, bool>> whereClause);
      new ISqlMapObject<T> Skip(int skip);
      new ISqlMapObject<T> Take(int take);
      ISqlMapObject<T> OrderBy(params Expression<Func<T, object>>[] expressions);
      ISqlMapObject<T> OrderByDescending(params Expression<Func<T, object>>[] expressions);
      ISqlMapObject<T> AddOrderBy(params Expression<Func<T, object>>[] expressions);
      ISqlMapObject<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions);
      new ISqlMapObject<T> Attach(ISqlExecuter executer);
      new ISqlMapObject<T> Union(ISqlReadObject sqlObject);
      new ISqlMapObject<T> Clone();
   }
}