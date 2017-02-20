using System.IO;

namespace Sql.Parser.Tokens
{
   public class JoinToken : SqlToken
   {

      public JoinToken(string expression, JoinType type, TableToken righTable) : base(expression)
      {
         JoinType = type;
         RighTable = righTable;
      }

      public JoinType JoinType { get; private set; }
      public TableToken RighTable { get; private set; }

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
