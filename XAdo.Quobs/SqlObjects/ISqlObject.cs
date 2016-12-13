using System;
using System.Linq.Expressions;

namespace XAdo.Quobs.SqlObjects
{
   public interface ISqlObject<T> : ISqlBuilder
   {
      ISqlObject<T> From(Expression<Func<T>> expression);
      object Apply(bool literals = false);
   }
}
