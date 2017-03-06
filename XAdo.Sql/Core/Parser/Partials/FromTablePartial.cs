using System;
using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class FromTablePartial : SqlPartial, ICloneable
   {

      private FromTablePartial() { }

      public FromTablePartial(TablePartial table)
         : base(table.ToString())
      {
         Table = table;
      }

      public TablePartial Table { get; protected set; }

      public override void Write(TextWriter w)
      {
         w.Write("FROM ");
         Table.Write(w);
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      public FromTablePartial Clone()
      {
         return new FromTablePartial{Expression = Expression, Table = Table.Clone()};
      }


   }
}