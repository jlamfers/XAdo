using System;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlWriteFromObject<TTable> : ISqlWriteObject
      where TTable : IDbTable
   {
      ISqlWriteFromObject<TTable> From(Expression<Func<TTable>> expression);
   }
}