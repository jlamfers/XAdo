using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class GroupByPartial : TemplatePartial, ICloneable
   {

      private GroupByPartial() { }

      public GroupByPartial(IList<ColumnPartial> columns, string expression)
         : base(expression)
      {
         Columns = columns.ToList().AsReadOnly();
      }

      public IList<ColumnPartial> Columns { get; private set; }

      public override void Write(TextWriter w)
      {
         w.Write("GROUP BY ");
         var comma = "";
         foreach (var c in Columns)
         {
            w.Write(comma);
            c.WriteNonAliased(w);
            comma = ", ";
         }
         w.Write(" ");
         base.Write(w);
      }

      object ICloneable.Clone()
      {
         return Clone();
      }
      public GroupByPartial Clone()
      {
         return new GroupByPartial{Columns = Columns.Select(c => c.Clone()).ToList().AsReadOnly(), Expression = Expression};
      }

   }
}