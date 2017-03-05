using System.Collections.Generic;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core.Interface
{
   public interface ISqlPredicateGenerator
   {
      SqlGeneratorResult Generate(Expression expression, IDictionary<string, string> fullnameToSqlExpressionMap, IDictionary<string, object> arguments = null);
   }

   public class SqlGeneratorResult
   {
      public SqlGeneratorResult(string sql, IDictionary<string, object> arguments)
      {
         Arguments = arguments;
         Sql = sql;
      }

      public string Sql { get; private set; }
      public IDictionary<string, object> Arguments { get; private set; }
   }
}