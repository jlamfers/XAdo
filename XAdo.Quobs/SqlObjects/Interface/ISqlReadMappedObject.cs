using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlReadMappedObject<TMapped> : ISqlFetchObject<TMapped>
   {
      new ISqlReadMappedObject<TMapped> Distinct();
      ISqlReadMappedObject<TMapped> Where(Expression<Func<TMapped, bool>> whereClause);
      new ISqlReadMappedObject<TMapped> Skip(int skip);
      new ISqlReadMappedObject<TMapped> Take(int take);
      ISqlReadMappedObject<TMapped> OrderBy(params Expression<Func<TMapped, object>>[] expressions);
      ISqlReadMappedObject<TMapped> OrderByDescending(params Expression<Func<TMapped, object>>[] expressions);
      ISqlReadMappedObject<TMapped> AddOrderBy(params Expression<Func<TMapped, object>>[] expressions);
      ISqlReadMappedObject<TMapped> AddOrderByDescending(params Expression<Func<TMapped, object>>[] expressions);
      new ISqlReadMappedObject<TMapped> Attach(ISqlConnection sqlConnection);
      new ISqlReadMappedObject<TMapped> Union(ISqlReadObject sqlObject);
      new ISqlReadMappedObject<TMapped> Clone();
   }
}