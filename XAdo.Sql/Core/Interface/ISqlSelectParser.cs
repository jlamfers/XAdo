using System.Collections.Generic;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Interface
{
   public interface ISqlSelectParser
   {
      IList<SqlPartial> Parse(string sql);
   }
}