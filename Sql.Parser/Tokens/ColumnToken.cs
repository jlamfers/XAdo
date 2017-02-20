using System.Collections.Generic;
using Microsoft.SqlServer.Server;

namespace Sql.Parser.Tokens
{
   public class ColumnToken : MultiPartAliasedToken
   {
      public ColumnToken(IList<string> parts, string alias) : base(parts, alias)
      {
      }
      public ColumnToken(MultiPartAliasedToken other)
         : base(other)
      {
         
      }

   }
}
