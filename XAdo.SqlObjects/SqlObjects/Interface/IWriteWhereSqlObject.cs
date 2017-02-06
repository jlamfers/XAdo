using System;
using System.Linq.Expressions;
using XAdo.SqlObjects.DbSchema;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public interface IWriteWhereSqlObject<TTable> : IWriteFromSqlObject<TTable>
      where TTable : IDbTable
   {
      new IWriteWhereSqlObject<TTable> From(Expression<Func<TTable>> expression);
      IWriteWhereSqlObject<TTable> Where(Expression<Func<TTable, bool>> whereExpression);
   }
}