using System;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Common;

namespace XAdo.Quobs
{
   public static class SessionExtensions
   {
      public static ISqlResource CreateSqlResource(this IAdoSession self, string sqlSelect, Type type = null)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var sqlResource = self.Context.GetInstance<ISqlResourceFactory>().Create(sqlSelect,type);
         sqlResource.GetBinder(self);
         return sqlResource;
      }
      public static ISqlResource<T> CreateSqlResource<T>(this IAdoSession self, string sqlSelect)
      {
         return self.CreateSqlResource(sqlSelect, typeof (T)).ToGeneric<T>();
      }
      public static ISqlResource CreateSqlResource(this IAdoSession self, Type type)
      {
         if (type == null) throw new ArgumentNullException("type");
         var sqlResource = self.Context.GetInstance<ISqlResourceFactory>().Create(type);
         return sqlResource;
      }
      public static ISqlResource CreateSqlResource<T>(this IAdoSession self)
      {
         return self.CreateSqlResource(typeof(T)).ToGeneric<T>();
      }

      public static IQuob<TEntity> Query<TEntity>(this IAdoSession self)
      {
         return self.Context.GetInstance<IQuob<TEntity>>()
            .CastTo<IAttachable>()
            .Attach(self)
            .CastTo<IQuob<TEntity>>();
      }
   }
}