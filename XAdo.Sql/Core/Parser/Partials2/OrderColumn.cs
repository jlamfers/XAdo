using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public class OrderColumn : SqlPartial
   {

      public OrderColumn(ColumnPartial column, bool descending)
         :base (column.ToString())
      {
         Column = column;
         Descending = @descending;
      }

      public ColumnPartial Column { get; private set; }
      public bool Descending { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         Column.WriteNonAliased(w,args);
         if (Descending)
         {
            w.Write(" DESC");
         }
      }

   }
}