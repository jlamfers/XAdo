using System;
using XAdo.Core.Impl;
using XAdo.Core.Interface;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Impl;
using XAdo.Quobs.Interface;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs
{
   public class DbContext : XAdoDbContext
   {

      public DbContext(Action<IXAdoContextInitializer> initializer, IXAdoClassBinder customClassBinder = null)
         : base(ctx => MyInitialize(ctx,initializer), customClassBinder)
      {
      }

      private static void MyInitialize(IXAdoContextInitializer context, Action<IXAdoContextInitializer> initializer)
      {
         context
            .BindSingleton<IXAdoCommandFactory, XAdoCommandFactoryImplEx>()
            .BindSingleton<IFilterParser,FilterParserImpl>()
            .BindSingleton<ISqlDialect,SqlServerDialect>()
            .BindSingleton<ISqlResourceRepository, SqlResourceRepositoryImpl>()
            .BindSingleton(typeof(IQuob<>),typeof(QuobImpl<>));

         context
            .KeepConnectionAlive(true)
            .EnableFieldBinding()
            .EnableEmittedDynamicTypes()
            .EnableAutoStringSanitize();

         initializer(context);
      }
   }
}
