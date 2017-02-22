using System.Collections.Generic;

namespace Sql.Parser.Partials
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
   }
}