using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlWriteWhereObject<TTable> : ISqlWriteObject
      where TTable : IDbTable
   {
      ISqlWriteWhereObject<TTable> Where(Expression<Func<TTable, bool>> whereExpression);
   }
}