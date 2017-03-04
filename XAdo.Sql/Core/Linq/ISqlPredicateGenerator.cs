using System.Collections.Generic;
using System.Linq.Expressions;

namespace XAdo.Quobs.Linq
{
   public interface ISqlPredicateGenerator
   {
      SqlGeneratorResult Generate(Expression expression, IDictionary<string, string> fullnameToSqlExpressionMap, IDictionary<string, object> arguments = null);
   }
}