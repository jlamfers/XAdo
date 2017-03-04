using System;
using XAdo.Core.Cache;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.Core
{
   public class SqlResourceFactory : ISqlResourceFactory
   {

      protected readonly LRUCache<object, SqlResource>
         SqlResourceCache = new LRUCache<object, SqlResource>("LRUCache.SqlResource.Size", 1000);

      private readonly ISqlDialect _dialect;
      private readonly IFilterParser _urlParser;

      public SqlResourceFactory(ISqlDialect dialect, IFilterParser urlParser)
      {
         if (dialect == null) throw new ArgumentNullException("dialect");
         if (urlParser == null) throw new ArgumentNullException("urlParser");
         _dialect = dialect;
         _urlParser = urlParser;
      }

      public virtual ISqlResource Create(string sql, Type type = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return SqlResourceCache.GetOrAdd(sql, x =>
         {
            var parser = new SqlSelectParser();
            var partials = parser.Parse(sql);
            var queryMap = new SqlResource(partials,_dialect,_urlParser);
            if (type != null)
            {
               queryMap.GetBinder(type);
            }
            return queryMap;
         });
      }

      public virtual ISqlResource Create(Type type)
      {
         var sqlSelectAttribute = type.GetAnnotation<SqlSelectAttribute>();
         if (sqlSelectAttribute == null)
         {
            throw new InvalidOperationException("Cannot get the SQL-select from type: " + type.Name + ". You must annotate the type with the [SqlSelect] attribute.");
         }
         return Create(sqlSelectAttribute.Sql, type);
      }
   }
}
