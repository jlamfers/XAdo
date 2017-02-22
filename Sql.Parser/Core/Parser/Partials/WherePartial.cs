using System.IO;

namespace Sql.Parser.Partials
{
   public class WherePartial : TemplatePartial
   {

      public WherePartial(string whereClause, string expression)
         : base(expression)
      {
         WhereClause = whereClause;
      }

      public string WhereClause { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("WHERE ");
         w.Write(WhereClause);
         w.Write(" ");
         base.Write(w, args);
      }

      public override string ToString()
      {
         return Expression.Length > 0 ? "WHERE " + WhereClause + " ?" + Expression
            : "WHERE " + WhereClause;
      }

   }
}