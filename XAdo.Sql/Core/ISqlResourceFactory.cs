using System;

namespace XAdo.Quobs.Core
{
   public interface ISqlResourceFactory
   {
      ISqlResource Create(string sql, Type type = null);
      ISqlResource Create(Type type);
   }

   public static class SqlResourceFactoryExtension
   {
      public static ISqlResource<T> Create<T>(this ISqlResourceFactory self)
      {
         return self.Create(typeof(T)).ToGeneric<T>();
      }
      public static ISqlResource<T> Create<T>(this ISqlResourceFactory self, string sql)
      {
         return self.Create(sql, typeof(T)).ToGeneric<T>();
      }

   }
}