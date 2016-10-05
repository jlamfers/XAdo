using System.Collections.Generic;

namespace XAdo.Quobs.Sql
{
   public interface ISqlBuilder
   {
      string GetSql();
      IDictionary<string, object> GetArguments();
   }
}