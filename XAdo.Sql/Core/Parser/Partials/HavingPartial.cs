using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class HavingPartial : TemplatePartial
   {
      public HavingPartial(string havingClause, string expression)
         : base(expression)
      {
         HavingClause = havingClause;
      }

      public string HavingClause { get; private set; }

      public override void Write(TextWriter w)
      {
         w.Write("HAVING ");
         w.Write(HavingClause);
         w.Write(" ");
         base.Write(w);
      }

   }
}