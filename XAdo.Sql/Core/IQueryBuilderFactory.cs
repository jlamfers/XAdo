using System;

namespace XAdo.Quobs.Core
{
   public interface IQueryBuilderFactory
   {
      IQueryBuilder Parse(string sql, Type type);
      IQueryBuilder<T> Parse<T>(string sql);
   }
}