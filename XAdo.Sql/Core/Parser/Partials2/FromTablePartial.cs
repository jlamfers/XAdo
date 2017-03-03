using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public class FromTablePartial : SqlPartial
   {

      public FromTablePartial(TablePartial table)
         : base(table.ToString())
      {
         Table = table;
      }

      public TablePartial Table { get; protected set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("FROM ");
         Table.Write(w,args);
      }

   }
}