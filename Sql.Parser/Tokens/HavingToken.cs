using System.IO;

namespace Sql.Parser.Tokens
{
   public class HavingToken : TemplateToken
   {

      public HavingToken(string havingClause, string expression)
         : base(expression)
      {
         HavingClause = havingClause;
      }

      public string HavingClause { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("HAVING ");
         w.Write(HavingClause);
         w.Write(" ");
         base.Write(w, args);
      }

      public override string ToString()
      {
         return Expression.Length > 0 ? "HAVING " + HavingClause + " ?" + Expression 
            : "HAVING " + HavingClause;
      }
   }
}