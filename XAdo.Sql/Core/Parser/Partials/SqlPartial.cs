using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class SqlPartial
   {
      protected SqlPartial()
      {
         // so that subclasses are be able to clone
      }

      public SqlPartial(string expression)
      {
         Expression = expression == null ? "" : expression.Trim();
      }

      public string Expression { get; protected set; }

      public virtual void Write(TextWriter w, object args)
      {
         w.Write(Expression);
      }

      public override string ToString()
      {
         using (var sw = new StringWriter())
         {
            Write(sw, null);
            return sw.GetStringBuilder().ToString();
         }
      }

   }
}
