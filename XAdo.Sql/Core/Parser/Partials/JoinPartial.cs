using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class JoinPartial : SqlPartial
   {

      public JoinPartial(string expression, JoinType type, TablePartial righTable) : base(expression)
      {
         JoinType = type;
         RighTable = righTable;
      }

      public JoinType JoinType { get; private set; }
      public TablePartial RighTable { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write(JoinType.ToString().ToUpper());
         w.Write(" ");
         w.Write(JoinType != JoinType.Inner ? "OUTER " : "");
         w.Write("JOIN ");
         RighTable.Write(w,args);
         w.Write(" ON ");
         base.Write(w, args);
      }

   }
}
