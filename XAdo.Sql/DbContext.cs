using System;
using XAdo.Core.Impl;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Impl;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Providers;

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
            .BindSingleton<IFilterParser, FilterParserImpl>()
            .BindSingleton<ISqlDialect, SqlServerDialect>()
            .BindSingleton<ISqlResourceRepository, SqlResourceRepositoryImpl>()
            .BindSingleton(typeof (IQuob<>), typeof (QuobImpl<>))
            .BindSingleton<ISqlScanner, SqlScannerImpl>()
            .BindSingleton<ISqlSelectParser, SqlSelectParserImpl>()
            .BindSingleton<ISqlPredicateGenerator, SqlPredicateGeneratorImpl>()
            .BindSingleton<ISqlBuilder, SqlBuilderImpl>();


         context
            .KeepConnectionAlive(true)
            .EnableFieldBinding()
            .EnableEmittedDynamicTypes()
            .EnableAutoStringSanitize();

         initializer(context);
      }
   }
}
