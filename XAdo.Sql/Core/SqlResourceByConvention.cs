using System;
using XAdo.Quobs.Core.Common;

namespace XAdo.Quobs.Core
{
   public class SqlResourceByConvention : ISqlResourceByConvention
   {
      private readonly ISqlResourceFactory _queryBuilderFactory;

      public SqlResourceByConvention(ISqlResourceFactory queryBuilderFactory)
      {
         _queryBuilderFactory = queryBuilderFactory;
      }

      public ISqlResource Create(Type type)
      {
         var sqlSelectAttribute = type.GetAnnotation<SqlSelectAttribute>();
         if (sqlSelectAttribute == null)
         {
            throw new InvalidOperationException("Cannot get the SQL-select from type: " + type.Name+". You must annotate the type with the [SqlSelect] attribute.");
         }
         return _queryBuilderFactory.Create(sqlSelectAttribute.Sql,type);
      }
   }
}