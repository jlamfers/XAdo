using System.Collections.Generic;

namespace Sql.Parser.Partials
{
   public class ColumnPartial : MultiPartAliasedPartial
   {
      public ColumnPartial(IList<string> parts, string alias) : base(parts, alias)
      {
      }
      public ColumnPartial(MultiPartAliasedPartial other)
         : base(other)
      {
         
      }

   }
}
