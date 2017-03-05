using System;

namespace XAdo.Quobs.Core.Interface
{
   public interface ISqlResourceRepository
   {
      ISqlResource Get(string sql, Type type = null);
      ISqlResource Get(Type type);
   }

   public static class SqlResourceRepositoryExtension
   {
      public static ISqlResource<T> Get<T>(this ISqlResourceRepository self)
      {
         return self.Get(typeof(T)).ToGeneric<T>();
      }
      public static ISqlResource<T> Get<T>(this ISqlResourceRepository self, string sql)
      {
         return self.Get(sql, typeof(T)).ToGeneric<T>();
      }

   }
}