using System.Collections.Generic;

namespace XAdo.Sql.Core.Parser.Partials
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

      public string Schema
      {
         get { return Parts.Count >= 3 ? Parts[0] : null; }
      }
      public string Table
      {
         get { return Parts.Count >= 2 ? Parts[Parts.Count - 2] : null; }
      }
      public string ColumnName
      {
         get { return Parts[Parts.Count - 1]; }
      }
      public string NameOrAlias
      {
         get { return !string.IsNullOrEmpty(Alias) ? Alias : ColumnName; }
      }


   }
}
