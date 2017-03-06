using System.IO;
using System.Runtime.InteropServices;

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

      public virtual void Write(TextWriter w)
      {
         w.Write(Expression);
      }

      public override string ToString()
      {
         using (var sw = new StringWriter())
         {
            Write(sw);
            return sw.GetStringBuilder().ToString();
         }
      }
   }
}
