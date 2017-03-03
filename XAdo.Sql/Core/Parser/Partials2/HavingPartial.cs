using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public class HavingPartial : TemplatePartial
   {
      protected HavingPartial(){}

      public HavingPartial(string havingClause, string expression)
         : base(expression)
      {
         HavingClause = havingClause;
      }

      public string HavingClause { get; protected set; }

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