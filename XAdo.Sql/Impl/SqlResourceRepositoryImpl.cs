using System;
using XAdo.Core;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Interface;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.Impl
{
   public class SqlResourceRepositoryImpl : ISqlResourceRepository
   {

      protected readonly LRUCache<object, SqlResourceImpl>
         SqlResourceCache = new LRUCache<object, SqlResourceImpl>("LRUCache.SqlResource.Size", 1000);

      private readonly ISqlDialect _dialect;
      private readonly IFilterParser _filterParser;

      public SqlResourceRepositoryImpl(ISqlDialect dialect, IFilterParser filterParser)
      {
         if (dialect == null) throw new ArgumentNullException("dialect");
         if (filterParser == null) throw new ArgumentNullException("filterParser");
         _dialect = dialect;
         _filterParser = filterParser;
      }

      public virtual ISqlResource Get(string sql, Type type = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return SqlResourceCache.GetOrAdd(sql, x =>
         {
            var parser = new SqlSelectParser();
            var partials = parser.Parse(sql);
            var queryMap = new SqlResourceImpl(partials,_dialect,_filterParser);
            if (type != null)
            {
               queryMap.GetBinder(type);
            }
            return queryMap;
         });
      }

      public virtual ISqlResource Get(Type type)
      {
         var sqlSelectAttribute = type.GetAnnotation<SqlSelectAttribute>();
         if (sqlSelectAttribute == null)
         {
            throw new InvalidOperationException("Cannot get the SQL-select from type: " + type.Name + ". You must annotate the type with the [SqlSelect] attribute.");
         }
         return Get(sqlSelectAttribute.Sql, type);
      }
   }
}
