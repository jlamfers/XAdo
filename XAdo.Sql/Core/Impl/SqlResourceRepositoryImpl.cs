using System;
using XAdo.Core;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;

namespace XAdo.Quobs.Core.Impl
{
   public class SqlResourceRepositoryImpl : ISqlResourceRepository
   {

      protected readonly LRUCache<object, SqlResourceImpl>
         SqlResourceCache = new LRUCache<object, SqlResourceImpl>("LRUCache.SqlResource.Size", 1000);

      private readonly ISqlDialect _dialect;
      private readonly IFilterParser _filterParser;
      private readonly ISqlSelectParser _sqlSelectParser;
      private readonly ISqlPredicateGenerator _sqlPredicateGenerator;

      public SqlResourceRepositoryImpl(ISqlDialect dialect, IFilterParser filterParser, ISqlSelectParser sqlSelectParser, ISqlPredicateGenerator sqlPredicateGenerator)
      {
         if (dialect == null) throw new ArgumentNullException("dialect");
         if (filterParser == null) throw new ArgumentNullException("filterParser");
         if (sqlSelectParser == null) throw new ArgumentNullException("sqlSelectParser");
         if (sqlPredicateGenerator == null) throw new ArgumentNullException("sqlPredicateGenerator");
         _dialect = dialect;
         _filterParser = filterParser;
         _sqlSelectParser = sqlSelectParser;
         _sqlPredicateGenerator = sqlPredicateGenerator;
      }

      public virtual ISqlResource Get(string sql, Type type = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return SqlResourceCache.GetOrAdd(sql, x =>
         {
            var partials = _sqlSelectParser.Parse(sql);
            var sqlResource = new SqlResourceImpl(partials, _dialect, _filterParser, _sqlPredicateGenerator);
            if (type != null)
            {
               sqlResource.GetBinder(type);
            }
            return sqlResource;
         });
      }

      public virtual ISqlResource Get(Type type)
      {
         var sqlSelectAttribute = type.GetAnnotation<SqlSelectAttribute>();
         if (sqlSelectAttribute == null)
         {
            throw new QuobException("Cannot get the SQL-select from type: " + type.Name + ". You must annotate the type with the [SqlSelect] attribute.");
         }
         return Get(sqlSelectAttribute.Sql, type);
      }
   }
}
