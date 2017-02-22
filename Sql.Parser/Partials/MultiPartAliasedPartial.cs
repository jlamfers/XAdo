using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sql.Parser.Common;

namespace Sql.Parser.Partials
{
   public class MultiPartAliasedPartial : SqlPartial
   {
      public MultiPartAliasedPartial(IList<string> parts, string alias)
         : base(string.Join(".", parts))
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

      public string Schema
      {
         get { return Parts.Count >= 3 ? Parts[0] : null; }
      }
      public string Table
      {
         get { return Parts.Count >= 2 ? Parts[Parts.Count - 2] : null; }
      }
      public string ColumnName
      {
         get { return Parts[Parts.Count - 1]; }
      }


      internal void SetAlias(string alias)
      {
         RawAlias = alias;
         Alias = alias;
      }

      public override void Write(TextWriter w, object args)
      {
         base.Write(w, args);
         if (RawAlias != null)
         {
            w.Write(" AS ");
            w.Write(RawAlias);
         }
      }

   }
}