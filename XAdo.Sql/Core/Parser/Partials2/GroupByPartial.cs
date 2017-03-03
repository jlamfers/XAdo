using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials2
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
            c.WriteNonAliased(w, args);
            comma = ", ";
         }
         w.Write(" ");
         base.Write(w, args);
      }

      public override string ToString()
      {
         using (var w = new StringWriter())
         {
            w.Write("GROUP BY ");
            var comma = "";
            foreach (var c in Columns)
            {
               w.Write(comma);
               c.WriteNonAliased(w, null);
               comma = ", ";
            }
            if (Expression.Length > 0)
            {
               w.Write(" ?");
               w.Write(Expression);
            }
            return w.GetStringBuilder().ToString();
         }

      }

   }
}