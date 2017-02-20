using System.Collections.Generic;

namespace Sql.Parser.Tokens
{
   public class TableToken : MultiPartAliasedToken
   {
      public TableToken(IList<string> parts, string alias)
         : base(parts, alias)
      {
      }

      public TableToken(MultiPartAliasedToken other)
         : base(other)
      {
         
      }
   }
}