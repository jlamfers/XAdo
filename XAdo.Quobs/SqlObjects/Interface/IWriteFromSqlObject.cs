using System;
using System.Linq.Expressions;
using XAdo.Quobs.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface IWriteFromSqlObject<TTable> : IWriteSqlObject
      where TTable : IDbTable
   {
      IWriteFromSqlObject<TTable> From(Expression<Func<TTable>> expression);
   }
}