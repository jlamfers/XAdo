using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sql.Parser.Tokens
{
   public class OrderByToken : TemplateToken
   {

      public OrderByToken(IList<OrderColumnToken> columns, string expression)
         : base(expression)
      {
         Columns = columns.ToList().AsReadOnly();
      }

      public IList<OrderColumnToken> Columns { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("ORDER BY ");
         var comma = "";
         foreach (var c in Columns)
         {
            w.Write(comma);
            c.Write(w, args);
            comma = ", ";
         }
         w.Write(" ");
         base.Write(w, args);
      }

      public override string ToString()
      {
         return Expression.Length > 0
            ? "ORDER BY " + (string.Join(", ", Columns) + "  ?" + Expression).Trim()
            : "ORDER BY " + string.Join(", ", Columns);
      }
   }
}