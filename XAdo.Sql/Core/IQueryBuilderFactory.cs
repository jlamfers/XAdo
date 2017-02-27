using System;

namespace XAdo.Sql.Core
{
   public interface IQueryBuilderFactory
   {
      IQueryBuilder Parse(string sql, Type type);
      IQueryBuilder<T> Parse<T>(string sql);
   }
}