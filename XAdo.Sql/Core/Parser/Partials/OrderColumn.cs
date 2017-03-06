using System;
using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class OrderColumnPartial : SqlPartial, ICloneable
   {

      private OrderColumnPartial() { }

      public OrderColumnPartial(ColumnPartial column, bool descending)
         :base (column.ToString())
      {
         Column = column;
         Descending = @descending;
      }

      public ColumnPartial Column { get; private set; }
      public bool Descending { get; private set; }

      public override void Write(TextWriter w)
      {
         Column.WriteNonAliased(w);
         if (Descending)
         {
            w.Write(" DESC");
         }
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      public OrderColumnPartial Clone()
      {
         return new OrderColumnPartial{Column = Column.Clone(),Descending = Descending,Expression = Expression};
      }

   }
}