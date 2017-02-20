using System;
using System.IO;

namespace Sql.Parser.Tokens
{
   public class WithToken : SqlToken
   {
      public WithToken(string expression, string alias)
         : base(expression)
      {
         if (alias == null) throw new ArgumentNullException("alias","Alias is required for WITH token");
         RawAlias = alias;
         Alias = alias.TrimQuotes();
      }

      public string Alias { get; private set; }
      public string RawAlias { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("WITH ");
         base.Write(w, args);
         w.Write(" AS ");
         w.Write(RawAlias);
      }

   }
}