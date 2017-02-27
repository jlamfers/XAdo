using System;

namespace XAdo.Quobs.Core
{
   public interface IQueryByConvention
   {
      IQueryBuilder GetQueryBuilder(Type type);
   }

   public static class QueryByConventionExtension
   {
      public static IQueryBuilder<T> GetQueryBuilder<T>(this IQueryByConvention self)
      {
         return self.GetQueryBuilder(typeof (T)).ToGeneric<T>();
      }
   }
}
