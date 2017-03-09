using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class JoinPartial : SqlPartial, ICloneable
   {

      private JoinPartial() { }

      public JoinPartial(string expression, JoinType type, TablePartial righTable, List<Tuple<ColumnPartial,ColumnPartial>> equiJoinColumns) : base(expression)
      {
         JoinType = type;
         RighTable = righTable;
         EquiJoinColumns = equiJoinColumns != null ? (IList<Tuple<ColumnPartial, ColumnPartial>>)equiJoinColumns.AsReadOnly() : new Tuple<ColumnPartial, ColumnPartial>[0];
      }

      public JoinType JoinType { get; private set; }
      public TablePartial RighTable { get; private set; }

      public IList<Tuple<ColumnPartial, ColumnPartial>> EquiJoinColumns { get;private set;}

      public override void Write(TextWriter w)
      {
         w.Write(JoinType.ToString().ToUpper());
         w.Write(" ");
         w.Write(JoinType != JoinType.Inner ? "OUTER " : "");
         w.Write("JOIN ");
         RighTable.Write(w);
         w.Write(" ON ");
         if (!EquiJoinColumns.Any())
         {
            base.Write(w);
         }
         else
         {
            var and = "";
            foreach (var c in EquiJoinColumns)
            {
               w.Write(and);
               c.Item1.WriteNonAliased(w);
               w.Write(" = ");
               c.Item2.WriteNonAliased(w);
               and = " AND ";
            }
         }
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      public JoinPartial Clone()
      {
         return new JoinPartial {
            Expression = Expression,
            JoinType = JoinType,
            RighTable = RighTable.Clone(),
            EquiJoinColumns = EquiJoinColumns.Any() 
               ? EquiJoinColumns.Select(c => Tuple.Create(c.Item1.Clone(), c.Item2.Clone())).ToList().AsReadOnly() 
               : EquiJoinColumns
         };
      }

   }
}
