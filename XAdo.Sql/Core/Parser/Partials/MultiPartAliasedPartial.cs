using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Sql.Core.Parser.Partials
{
   public class MultiPartAliasedPartial : SqlPartial
   {
      public MultiPartAliasedPartial(IList<string> parts, string alias)
         : base(string.Join(Constants.SpecialChars.COLUMN_SEP_STR, parts))
      {
         RawParts = parts.ToList().AsReadOnly();
         Parts = parts.Select(s => s.TrimQuotes()).ToList().AsReadOnly();
         RawAlias = alias;
         Alias = alias.TrimQuotes();
      }

      public MultiPartAliasedPartial(MultiPartAliasedPartial other)
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

      internal void SetAlias(string alias)
      {
         RawAlias = alias;
         Alias = alias;
      }

      public override void Write(TextWriter w, object args)
      {
         w.Write(string.Join(Constants.SpecialChars.COLUMN_SEP_STR, RawParts));
         if (RawAlias != null)
         {
            w.Write(" AS ");
            w.Write(RawAlias);
         }
      }

   }
}