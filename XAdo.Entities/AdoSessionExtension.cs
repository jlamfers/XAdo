using System.Collections.Generic;
using XAdo.Core.Interface;
using XAdo.Entities.Sql;

namespace XAdo.Entities
{
   public static class Extensions
   {
      internal static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T) self;
      }

      public static IEnumerable<T> Select<T>(this IAdoSession self)
      {
         return self.Query<T>(new SqlBuilder(typeof(T)).SqlSelect);
      } 
   }
}
