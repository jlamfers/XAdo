using System;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Common;

namespace XAdo.Quobs
{
   public static class SessionExtensions
   {
      public static ISqlResource GetQueryBuilder(this IAdoSession self, string sqlSelect, Type type = null)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var builder = self.Context.GetInstance<ISqlResourceFactory>().Create(sqlSelect,type);
         builder.GetBinder(self);
         return builder;
      }

      public static ISqlResource GetQueryBuilder<T>(this IAdoSession self, string sqlSelect)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var builder = self.Context.GetInstance<ISqlResourceFactory>().Create<T>(sqlSelect);
         builder.GetBinder(typeof(T));
         return builder;
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