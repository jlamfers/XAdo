using System.IO;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core.DbSchema
{
   public class JoinExpressionParser : ExpressionVisitor
   {
      private StringWriter _sw;
      public string Parse(Expression expression)
      {
         using (_sw = new StringWriter())
         {
            return _sw.ToString();
         }
      }
   }
}
