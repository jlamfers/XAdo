using XAdo.Core;
using XAdo.Core.Interface;

namespace XAdo.Sql
{
   public static class SessionExtensions
   {
      public static IQuob<TEntity> Query<TEntity>(this IAdoSession self)
      {
         return self.Context.GetInstance<IQuob<TEntity>>()
            .CastTo<Quob<TEntity>>()
            .Attach(self);
      }
   }
}
