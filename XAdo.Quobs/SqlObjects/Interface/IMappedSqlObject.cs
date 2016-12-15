using System;
using System.Linq.Expressions;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public interface IMappedSqlObject : IReadSqlObject { }

   public interface IMappedSqlObject<TMapped> : IMappedSqlObject, IFetchSqlObject<TMapped>
   {
      new IMappedSqlObject<TMapped> Distinct();
      IMappedSqlObject<TMapped> Where(Expression<Func<TMapped, bool>> whereClause);
      new IMappedSqlObject<TMapped> Skip(int skip);
      new IMappedSqlObject<TMapped> Take(int take);
      IMappedSqlObject<TMapped> OrderBy(params Expression<Func<TMapped, object>>[] expressions);
      IMappedSqlObject<TMapped> OrderByDescending(params Expression<Func<TMapped, object>>[] expressions);
      IMappedSqlObject<TMapped> AddOrderBy(params Expression<Func<TMapped, object>>[] expressions);
      IMappedSqlObject<TMapped> AddOrderByDescending(params Expression<Func<TMapped, object>>[] expressions);
      new IMappedSqlObject<TMapped> Attach(ISqlConnection sqlConnection);
      new IMappedSqlObject<TMapped> Union(IReadSqlObject sqlObject);
      new IMappedSqlObject<TMapped> Clone();
   }
}