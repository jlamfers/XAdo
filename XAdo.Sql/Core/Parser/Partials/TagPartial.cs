using System;
using System.IO;
using System.Linq;

namespace XAdo.Sql.Core.Parser.Partials
{
   public class TagPartial : SqlPartial
   {
      public TagPartial(string expression)
         : base(expression)
      {
         Tag = expression.Split(new []{"//"}, StringSplitOptions.None).First().Trim();
      }

      public string Tag { get;private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("-- >");
         base.Write(w, args);
      }

   }
}