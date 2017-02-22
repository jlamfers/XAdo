using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sql.Parser.Common;

namespace Sql.Parser.Partials
{
   public class OrderColumnPartial : SqlPartial
   {
      public OrderColumnPartial(IList<string> parts, string order)
         : base(string.Join(".", parts))
      {
         Parts = parts.Select(s => s.TrimQuotes()).ToList().AsReadOnly();
         Descending = order != null && order.ToUpper() == "DESC";
      }

      public bool Descending { get; private set; }
      public IList<string> Parts { get; private set; }

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