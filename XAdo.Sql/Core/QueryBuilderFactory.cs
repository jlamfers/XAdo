using System;
using XAdo.Core.Cache;
using XAdo.Sql.Core.Parser;
using XAdo.Sql.Dialects;
using XAdo.Sql.Linq;

namespace XAdo.Sql.Core
{
   public class QueryBuilderFactory : IQueryBuilderFactory
   {

      protected readonly LRUCache<object, QueryBuilder>
         QueryBuilderCache = new LRUCache<object, QueryBuilder>("LRUCache.QueryBuilder.Size", 1000);



      private readonly ISqlDialect _dialect;
      private readonly IUrlExpressionParser _urlParser;

      public QueryBuilderFactory(ISqlDialect dialect, IUrlExpressionParser urlParser)
      {
         if (dialect == null) throw new ArgumentNullException("dialect");
         if (urlParser == null) throw new ArgumentNullException("urlParser");
         _dialect = dialect;
         _urlParser = urlParser;
      }

      public virtual IQueryBuilder Parse(string sql, Type type)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return QueryBuilderCache.GetOrAdd(sql, x =>
         {
            var parser = new SqlSelectParser();
            var partials = parser.Parse(sql);
            var queryMap = new QueryBuilder(partials,_dialect,_urlParser);
            if (type != null)
            {
               queryMap.GetBinder(type);
            }
            return queryMap;
         });
      }

      public virtual IQueryBuilder<T> Parse<T>(string sql)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return QueryBuilderCache.GetOrAdd(sql, x =>
         {
            var parser = new SqlSelectParser();
            var partials = parser.Parse(sql);
            var queryMap = new QueryBuilder(partials, _dialect, _urlParser);
            queryMap.GetBinder(typeof (T));
            return queryMap;
         }).ToGeneric<T>();
      }

   }
}
