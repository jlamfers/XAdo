using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class TemplatePartial : SqlPartial
   {
      private class KeepFormatting { }
      public static readonly object AsTemplate = new KeepFormatting();
      
      protected TemplatePartial() { }

      public TemplatePartial(string expression) : base(expression)
      {
      }

      public override void Write(TextWriter w, object args)
      {
         if (string.IsNullOrEmpty(Expression)) return;

         if (args is KeepFormatting)
         {
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
            w.Write(Expression);
         }
         else
         {
            w.Write(Expression.FormatTemplate(args));
         }
      }

      public override string ToString()
      {
         return "--$" + Expression;
      }

   }
}
