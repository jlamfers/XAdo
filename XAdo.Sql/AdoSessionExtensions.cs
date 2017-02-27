using System;
using XAdo.Core.Interface;
using XAdo.Sql.Core;
using XAdo.Sql.Core.Common;

namespace XAdo.Sql
{
   public static class AdoSessionExtensions
   {
      public static IQueryBuilder GetQueryBuilder(this IAdoSession self, string sqlSelect, Type type = null)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var builder = self.Context.GetInstance<IQueryBuilderFactory>().Parse(sqlSelect,type);
         return builder;
      }

      public static IQueryBuilder GetQueryBuilder<T>(this IAdoSession self, string sqlSelect)
      {
         if (sqlSelect == null) throw new ArgumentNullException("sqlSelect");
         var builder = self.Context.GetInstance<IQueryBuilderFactory>().Parse<T>(sqlSelect);
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