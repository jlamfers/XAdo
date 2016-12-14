using System;
using XAdo.Core.Interface;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class SqlObjectsContext : AdoContext
   {
      public SqlObjectsContext(string connectionStringName) : base(connectionStringName)
      {
      }

      public SqlObjectsContext(Action<IAdoContextInitializer> initializer, IAdoClassBinder customClassBinder = null) 
         : base(WrapInitializer(initializer), customClassBinder)
      {
      }

      private static Action<IAdoContextInitializer> WrapInitializer(Action<IAdoContextInitializer> initializer)
      {
         return i =>
         {
            i.BindSingleton<ISqlObjectFactory, SqlObjectFactory>();
            initializer(i);
         };
      }
   }
}
