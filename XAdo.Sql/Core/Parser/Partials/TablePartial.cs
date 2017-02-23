using System.Collections.Generic;

namespace XAdo.Sql.Core.Parser.Partials
{
   public class TablePartial : MultiPartAliasedPartial
   {
      public TablePartial(IList<string> parts, string alias)
         : base(parts, alias)
      {
      }

      public TablePartial(MultiPartAliasedPartial other)
         : base(other)
      {
         
      }

      public string Schema
      {
         get { return Parts.Count >= 2 ? Parts[Parts.Count - 2] : null; }
      }
      public string TableName
      {
         get { return Parts[Parts.Count - 1]; }
      }

   }
}