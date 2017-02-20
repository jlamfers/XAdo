using System.IO;

namespace Sql.Parser.Tokens
{
   public class LineCommentToken : SqlToken
   {
      public LineCommentToken(string expression) : base(expression)
      {
         
      }

      public override void Write(TextWriter w, object args)
      {
         w.Write("-- ");
         base.Write(w, args);
      }
   }
}
