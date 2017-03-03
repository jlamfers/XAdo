using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class TemplatePartial : SqlPartial
   {
      protected TemplatePartial() { }

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
