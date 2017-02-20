using System.IO;

namespace Sql.Parser.Tokens
{
   public class TemplateToken : SqlToken
   {
      public TemplateToken(string expression) : base(expression)
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
