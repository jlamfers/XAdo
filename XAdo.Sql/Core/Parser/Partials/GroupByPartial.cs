using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Sql.Core.Parser.Partials
{
   public class GroupByPartial : TemplatePartial
   {

      public GroupByPartial(IList<ColumnPartial> columns, string expression)
         : base(expression)
      {
         Columns = columns.ToList().AsReadOnly();
      }

      public IList<ColumnPartial> Columns { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("GROUP BY ");
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
            ? "GROUP BY " + (string.Join(", ", Columns) + "  ?" + Expression).Trim()
            : "GROUP BY " + string.Join(", ", Columns);
      }
   }
}