using System.IO;

namespace Sql.Parser.Partials
{
   public class LineCommentPartial : SqlPartial
   {
      public LineCommentPartial(string expression) : base(expression)
      {
         
      }

      public override void Write(TextWriter w, object args)
      {
         w.Write("-- ");
         base.Write(w, args);
      }
   }
}
