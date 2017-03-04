using System.Collections.Generic;

namespace XAdo.Quobs.Linq
{
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