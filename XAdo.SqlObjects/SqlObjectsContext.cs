using System;
using XAdo.Core.Interface;
using XAdo.SqlObjects.SqlObjects;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects
{
   public class SqlObjectsContext : XAdoDbContext
   {
      public SqlObjectsContext(string connectionStringName) : base(connectionStringName)
      {
      }

      public SqlObjectsContext(Action<IXAdoContextInitializer> initializer, IXAdoClassBinder customClassBinder = null) 
         : base(WrapInitializer(initializer), customClassBinder)
      {
      }

      private static Action<IXAdoContextInitializer> WrapInitializer(Action<IXAdoContextInitializer> initializer)
      {
         return i =>
         {
            i
               .BindSingleton<ISqlObjectFactory, SqlObjectFactory>()
               .KeepConnectionAlive(true);

            initializer(i);
         };
      }
   }
}
