using System;
using System.Linq.Expressions;

namespace XAdo.Quobs.SqlObjects
{
   public interface ISqlWhereObject<T>
   {
      ISqlObject<T> Where(Expression<Func<T, bool>> expression);
   }
}