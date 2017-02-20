using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sql.Parser.Tokens
{
   public class MultiPartAliasedToken : SqlToken
   {
      public MultiPartAliasedToken(IList<string> parts, string alias)
         : base(string.Join(".", parts))
      {
         RawParts = parts.ToList().AsReadOnly();
         Parts = parts.Select(s => s.TrimQuotes()).ToList().AsReadOnly();
         RawAlias = alias;
         Alias = alias.TrimQuotes();
      }

      public MultiPartAliasedToken(MultiPartAliasedToken other)
         : base(other.Expression)
      {
         Alias = other.Alias;
         RawAlias = other.RawAlias;
         Parts = other.Parts;
         RawParts = other.RawParts;
      }

      public IList<string> RawParts { get; internal set; }
      public string RawAlias { get; internal set; }
      public string Alias { get; internal set; }
      public IList<string> Parts { get; internal set; }

      public override void Write(TextWriter w, object args)
      {
         base.Write(w, args);
         if (Alias != null)
         {
            w.Write(" AS ");
            w.Write(RawAlias);
         }
      }

   }
}