using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface IWriteWhereSqlObject<TTable> : IWriteFromSqlObject<TTable>
      where TTable : IDbTable
   {
      new IWriteWhereSqlObject<TTable> From(Expression<Func<TTable>> expression);
      IWriteWhereSqlObject<TTable> Where(Expression<Func<TTable, bool>> whereExpression);
   }
}