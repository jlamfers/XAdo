using System;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Interface;

namespace XAdo.Quobs
{
   public static class XAdoSessionExtensions
   {
      public static ISqlResource GetSqlResource(this IXAdoSession self, string sqlSelect, Type type = null)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var sqlResource = self.Context.GetInstance<ISqlResourceRepository>().Get(sqlSelect,type);
         sqlResource.GetBinder(self);
         return sqlResource;
      }
      public static ISqlResource<T> GetSqlResource<T>(this IXAdoSession self, string sqlSelect)
      {
         return self.GetSqlResource(sqlSelect, typeof (T)).ToGeneric<T>();
      }
      public static ISqlResource GetSqlResource(this IXAdoSession self, Type type)
      {
         if (type == null) throw new ArgumentNullException("type");
         var sqlResource = self.Context.GetInstance<ISqlResourceRepository>().Get(type);
         return sqlResource;
      }
      public static ISqlResource GetSqlResource<T>(this IXAdoSession self)
      {
         return self.GetSqlResource(typeof(T)).ToGeneric<T>();
      }

      public static IQuob<TEntity> Query<TEntity>(this IXAdoSession self)
      {
         return self.Context.GetInstance<IQuob<TEntity>>()
            .CastTo<IAttachable>()
            .Attach(self)
            .CastTo<IQuob<TEntity>>();
      }
   }
}