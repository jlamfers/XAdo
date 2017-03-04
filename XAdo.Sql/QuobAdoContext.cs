using System;
using XAdo.Core.Impl;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Impl;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs
{
   public class QuobAdoContext : AdoContext
   {

      public QuobAdoContext(Action<IAdoContextInitializer> initializer, IAdoClassBinder customClassBinder = null)
         : base(ctx => MyInitialize(ctx,initializer), customClassBinder)
      {
      }

      private static void MyInitialize(IAdoContextInitializer context, Action<IAdoContextInitializer> initializer)
      {
         context
            .BindSingleton<IAdoCommandFactory, AdoCommandFactoryImplEx>()
            .BindSingleton<IFilterParser,FilterParser>()
            .BindSingleton<ISqlDialect,SqlServerDialect>()
            .BindSingleton<ISqlResourceFactory, SqlResourceFactory>()
            .BindSingleton(typeof(IQuob<>),typeof(Quob<>));

         context
            .KeepConnectionAlive(true)
            .EnableFieldBinding()
            .EnableEmittedDynamicTypes()
            .EnableAutoStringSanitize();

         initializer(context);
      }
   }
}
