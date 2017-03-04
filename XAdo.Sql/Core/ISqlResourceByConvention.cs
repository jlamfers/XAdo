using System;

namespace XAdo.Quobs.Core
{
   public interface ISqlResourceByConvention
   {
      ISqlResource Create(Type type);
   }

   public static class SqlResourceByConventionExtension
   {
      public static ISqlResource<T> Create<T>(this ISqlResourceByConvention self)
      {
         return self.Create(typeof (T)).ToGeneric<T>();
      }
   }
}
