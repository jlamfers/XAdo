using System.IO;

namespace Sql.Parser.Partials
{
   public class TemplatePartial : SqlPartial
   {
      public TemplatePartial(string expression) : base(expression)
      {
      }

      public override void Write(TextWriter w, object args)
      {
         w.Write(Expression.FormatTemplate(args));
      }

      public override string ToString()
      {
         return "--$" + Expression;
      }
   }
}
