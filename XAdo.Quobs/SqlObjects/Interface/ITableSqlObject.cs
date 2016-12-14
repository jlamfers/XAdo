using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ITableSqlObject : IReadSqlObject { }

   public interface ITableSqlObject<TTable> : ITableSqlObject, IFetchSqlObject<TTable>
      where TTable : IDbTable
   {
      IMappedSqlObject<TMapped> Map<TMapped>(Expression<Func<TTable, TMapped>> mapExpression);
      new ITableSqlObject<TTable> Distinct();
      ITableSqlObject<TTable> Where(Expression<Func<TTable, bool>> whereClause);
      ITableSqlObject<TTable> Having(Expression<Func<TTable, bool>> havingClause);
      new ITableSqlObject<TTable> Skip(int skip);
      new ITableSqlObject<TTable> Take(int take);
      ITableSqlObject<TTable> OrderBy(params Expression<Func<TTable, object>>[] expressions);
      ITableSqlObject<TTable> OrderByDescending(params Expression<Func<TTable, object>>[] expressions);
      ITableSqlObject<TTable> AddOrderBy(params Expression<Func<TTable, object>>[] expressions);
      ITableSqlObject<TTable> AddOrderByDescending(params Expression<Func<TTable, object>>[] expressions);
      new ITableSqlObject<TTable> Union(IReadSqlObject sqlObject);
      ITableSqlObject<TTable> GroupBy(params Expression<Func<TTable, object>>[] expressions);
      new ITableSqlObject<TTable> Clone();
   }
}