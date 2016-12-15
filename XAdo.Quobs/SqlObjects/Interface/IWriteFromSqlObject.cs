using System;
using System.Linq.Expressions;
using XAdo.SqlObjects.DbSchema;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public interface IWriteFromSqlObject<TTable> : IWriteSqlObject
      where TTable : IDbTable
   {
      IWriteFromSqlObject<TTable> From(Expression<Func<TTable>> expression);
   }
}