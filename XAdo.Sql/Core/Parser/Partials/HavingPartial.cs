using System.IO;

namespace XAdo.Sql.Core.Parser.Partials
{
   public class HavingPartial : TemplatePartial
   {

      public HavingPartial(string havingClause, string expression)
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