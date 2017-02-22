using System;
using System.IO;
using Sql.Parser.Common;

namespace Sql.Parser.Partials
{
   public class WithPartial : SqlPartial
   {
      public WithPartial(string expression, string alias)
         : base(expression)
      {
         if (alias == null) throw new ArgumentNullException("alias","Alias is required for WITH partial");
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