using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlTableObject<TTable> : ISqlFetchObject<TTable>
      where TTable : IDbTable
   {
      ISqlMapObject<TMapped> Select<TMapped>(Expression<Func<TTable, TMapped>> mapExpression);
      new ISqlTableObject<TTable> Distinct();
      ISqlTableObject<TTable> Where(Expression<Func<TTable, bool>> whereClause);
      ISqlTableObject<TTable> Having(Expression<Func<TTable, bool>> havingClause);
      new ISqlTableObject<TTable> Skip(int skip);
      new ISqlTableObject<TTable> Take(int take);
      ISqlTableObject<TTable> OrderBy(params Expression<Func<TTable, object>>[] expressions);
      ISqlTableObject<TTable> OrderByDescending(params Expression<Func<TTable, object>>[] expressions);
      ISqlTableObject<TTable> AddOrderBy(params Expression<Func<TTable, object>>[] expressions);
      ISqlTableObject<TTable> AddOrderByDescending(params Expression<Func<TTable, object>>[] expressions);
      new ISqlTableObject<TTable> Union(ISqlReadObject sqlObject);
      ISqlTableObject<TTable> GroupBy(params Expression<Func<TTable, object>>[] expressions);
      new ISqlTableObject<TTable> Clone();
   }
}