using System;
using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class JoinPartial : SqlPartial, ICloneable
   {

      private JoinPartial() { }

      public JoinPartial(string expression, JoinType type, TablePartial righTable) : base(expression)
      {
         JoinType = type;
         RighTable = righTable;
      }

      public JoinType JoinType { get; private set; }
      public TablePartial RighTable { get; private set; }

      public override void Write(TextWriter w)
      {
         w.Write(JoinType.ToString().ToUpper());
         w.Write(" ");
         w.Write(JoinType != JoinType.Inner ? "OUTER " : "");
         w.Write("JOIN ");
         RighTable.Write(w);
         w.Write(" ON ");
         base.Write(w);
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      public JoinPartial Clone()
      {
         return new JoinPartial{Expression = Expression, JoinType = JoinType, RighTable = RighTable.Clone()};
      }

   }
}
