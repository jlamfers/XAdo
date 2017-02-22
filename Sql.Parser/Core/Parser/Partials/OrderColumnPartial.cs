using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sql.Parser.Common;
using Sql.Parser.Parser;

namespace Sql.Parser.Partials
{
   public class OrderColumnPartial : SqlPartial
   {
      public OrderColumnPartial(IList<string> parts, string order)
         : base(string.Join(Constants.SpecialChars.COLUMN_SEP_STR, parts))
      {
         Parts = parts.Select(s => s.TrimQuotes()).ToList().AsReadOnly();
         Descending = order != null && order.ToUpper() == "DESC";
      }

      public bool Descending { get; private set; }
      public IList<string> Parts { get; private set; }

      public string Schema
      {
         get { return Parts.Count >= 3 ? Parts[0] : null; }
      }
      public string Table
      {
         get { return Parts.Count >= 2 ? Parts[Parts.Count - 2] : null; }
      }
      public string Name
      {
         get { return Parts[Parts.Count - 1]; }
      }

      public override void Write(TextWriter w, object args)
      {
         base.Write(w, args);
         if (Descending)
         {
            w.Write(" DESC");
         }
      }

   }
}