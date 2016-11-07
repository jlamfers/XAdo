using System.Collections.Generic;

namespace XAdo.Quobs.Core
{
   public interface ISqlBuilder
   {
      string GetSql();
      IDictionary<string, object> GetArguments();
   }
}