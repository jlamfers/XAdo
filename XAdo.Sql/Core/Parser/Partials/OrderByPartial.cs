using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class OrderByPartial : TemplatePartial, ICloneable
   {

      private OrderByPartial() { }

      public OrderByPartial(IList<OrderColumnPartial> columns, string expression)
         : base(expression)
      {
         Columns = columns.ToList().AsReadOnly();
      }

      public IList<OrderColumnPartial> Columns { get; private set; }

      public override void Write(TextWriter w)
      {
         w.Write("ORDER BY ");
         var comma = "";
         foreach (var c in Columns)
         {
            w.Write(comma);
            c.Write(w);
            comma = ", ";
         }
         w.Write(" ");
         base.Write(w);
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      public OrderByPartial Clone()
      {
         return new OrderByPartial{Columns = Columns.Select(c => c.Clone()).ToList().AsReadOnly(), Expression = Expression};
      }

   }
}