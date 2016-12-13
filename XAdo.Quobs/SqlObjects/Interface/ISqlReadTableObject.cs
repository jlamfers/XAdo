using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlReadTableObject<TTable> : ISqlFetchObject<TTable>
      where TTable : IDbTable
   {
      ISqlReadMappedObject<TMapped> Map<TMapped>(Expression<Func<TTable, TMapped>> mapExpression);
      new ISqlReadTableObject<TTable> Distinct();
      ISqlReadTableObject<TTable> Where(Expression<Func<TTable, bool>> whereClause);
      ISqlReadTableObject<TTable> Having(Expression<Func<TTable, bool>> havingClause);
      new ISqlReadTableObject<TTable> Skip(int skip);
      new ISqlReadTableObject<TTable> Take(int take);
      ISqlReadTableObject<TTable> OrderBy(params Expression<Func<TTable, object>>[] expressions);
      ISqlReadTableObject<TTable> OrderByDescending(params Expression<Func<TTable, object>>[] expressions);
      ISqlReadTableObject<TTable> AddOrderBy(params Expression<Func<TTable, object>>[] expressions);
      ISqlReadTableObject<TTable> AddOrderByDescending(params Expression<Func<TTable, object>>[] expressions);
      new ISqlReadTableObject<TTable> Union(ISqlReadObject sqlObject);
      ISqlReadTableObject<TTable> GroupBy(params Expression<Func<TTable, object>>[] expressions);
      new ISqlReadTableObject<TTable> Clone();
   }
}