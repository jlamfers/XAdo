using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Sql.Parser.Tokens
{
   public class SqlToken
   {
      public SqlToken(string expression)
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

   public static class SqlTokenExtensions
   {
      public static void Format(this IList<SqlToken> self, StringWriter w, object args)
      {
         var sb = w.GetStringBuilder();
         var pos = sb.Length;
         foreach (var t in self)
         {
            t.Write(w,args);
            if (sb.Length > pos)
            {
               w.WriteLine();
            }
            pos = sb.Length;
         }
      }

      public static string Format(this IList<SqlToken> self, object args)
      {
         using (var sw = new StringWriter())
         {
            self.Format(sw, args);
            return sw.GetStringBuilder().ToString();
         }
      }

   }
}