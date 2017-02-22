using System.IO;

namespace Sql.Parser.Core.Parser.Partials
{
   public class SqlPartial
   {
      public SqlPartial(string expression)
      {
         expression = expression ?? "";
         Expression = expression.Trim();
      }

      public string Expression { get; private set; }

      public override string ToString()
      {
         using (var sw = new StringWriter())
         {
            Write(sw,null);
            return sw.GetStringBuilder().ToString();
         }
      }

      public virtual void Write(TextWriter w, object args)
      {
         w.Write(Expression);
      }

   }
}