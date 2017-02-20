using System.Collections.Generic;
using System.IO;

namespace Sql.Parser.Tokens
{
   public class FromTableToken : TableToken
   {
      public FromTableToken(IList<string> parts, string alias) : base(parts, alias)
      {
      }

      public FromTableToken(MultiPartAliasedToken other) : base(other)
      {
      }

      public override void Write(TextWriter w, object args)
      {
         w.Write("FROM ");
         base.Write(w, args);
      }

   }
}