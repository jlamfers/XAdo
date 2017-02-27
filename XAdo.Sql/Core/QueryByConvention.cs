using System;
using XAdo.Quobs.Core.Common;

namespace XAdo.Quobs.Core
{
   public class QueryByConvention : IQueryByConvention
   {
      private readonly IQueryBuilderFactory _queryBuilderFactory;

      public QueryByConvention(IQueryBuilderFactory queryBuilderFactory)
      {
         _queryBuilderFactory = queryBuilderFactory;
      }

      public IQueryBuilder GetQueryBuilder(Type type)
      {
         var sqlSelectAttribute = type.GetAnnotation<SqlSelectAttribute>();
         if (sqlSelectAttribute == null)
         {
            throw new InvalidOperationException("Cannot get the SQL-select from type: " + type.Name+". You must annotate the type with the [SqlSelect] attribute.");
         }
         return _queryBuilderFactory.Parse(sqlSelectAttribute.Sql,type);
      }
   }
}