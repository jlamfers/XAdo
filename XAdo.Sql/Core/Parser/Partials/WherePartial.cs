using System;
using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class WherePartial : TemplatePartial
   {
      public WherePartial(string whereClause, string expression)
         : base(expression)
      {
         WhereClause = whereClause;
      }

      public string WhereClause { get; private set; }

      public override void Write(TextWriter w)
      {
         w.Write("WHERE (");
         w.Write(WhereClause);
         w.Write(") ");
         base.Write(w);
      }

   }
}