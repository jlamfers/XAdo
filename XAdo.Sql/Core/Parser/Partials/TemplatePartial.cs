using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class TemplatePartial : SqlPartial
   {
     
      protected TemplatePartial() { }

      public TemplatePartial(string expression) : base(expression)
      {
      }

      public override void Write(TextWriter w)
      {
         if (string.IsNullOrEmpty(Expression)) return;

         var sw = w as StringWriter;
         if (sw != null)
         {
            var sb = sw.GetStringBuilder();
            if (sb.Length > 0 && sb[sb.Length - 1] != '\n')
            {
               w.WriteLine();
            }
         }
         else
         {
            w.WriteLine();
         }
         w.Write("-- $");
         w.Write(Expression);
      }

   }
}
