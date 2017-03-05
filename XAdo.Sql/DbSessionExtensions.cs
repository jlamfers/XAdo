using System;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Interface;

namespace XAdo.Quobs
{
   public static class DbSessionExtensions
   {
      public static ISqlResource GetSqlResource(this IXAdoDbSession self, string sqlSelect, Type type = null)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var sqlResource = self.Context.GetInstance<ISqlResourceRepository>().Get(sqlSelect,type);
         sqlResource.GetBinder(self);
         return sqlResource;
      }
      public static ISqlResource<T> GetSqlResource<T>(this IXAdoDbSession self, string sqlSelect)
      {
         return self.GetSqlResource(sqlSelect, typeof (T)).ToGeneric<T>();
      }
      public static ISqlResource GetSqlResource(this IXAdoDbSession self, Type type)
      {
         if (type == null) throw new ArgumentNullException("type");
         var sqlResource = self.Context.GetInstance<ISqlResourceRepository>().Get(type);
         return sqlResource;
      }
      public static ISqlResource GetSqlResource<T>(this IXAdoDbSession self)
      {
         return self.GetSqlResource(typeof(T)).ToGeneric<T>();
      }

      public static IQuob<TEntity> Query<TEntity>(this IXAdoDbSession self)
      {
         return self.Context.GetInstance<IQuob<TEntity>>()
            .CastTo<IAttachable>()
            .Attach(self)
            .CastTo<IQuob<TEntity>>();
      }
   }
}