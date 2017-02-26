using System;
using XAdo.Core.Interface;
using XAdo.Sql.Core;
using XAdo.Sql.Dialects;
using XAdo.Sql.Linq;

namespace XAdo.Sql
{
   public class SqlAdoContext : AdoContext
   {
      public SqlAdoContext(string connectionStringName) : base(connectionStringName)
      {
      }

      public SqlAdoContext(Action<IAdoContextInitializer> initializer, IAdoClassBinder customClassBinder = null)
         : base(ctx => MyInitialize(ctx,initializer), customClassBinder)
      {
      }

      private static void MyInitialize(IAdoContextInitializer context, Action<IAdoContextInitializer> initializer)
      {
         context
            .BindSingleton<IUrlExpressionParser,UrlExpressionParser>()
            .BindSingleton<ISqlDialect,SqlServerDialect>()
            .BindSingleton<IQueryBuilderFactory, QueryBuilderFactory>();

         initializer(context);
      }
   }
}
