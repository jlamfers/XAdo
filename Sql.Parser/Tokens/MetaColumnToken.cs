using System.Collections.Generic;

namespace Sql.Parser.Tokens
{
   public class MetaColumnToken : ColumnToken
   {
      public MetaColumnToken(IList<string> parts, string alias, ColumnMap map, ColumnMeta meta) : base(parts, alias)
      {
         Meta = meta;
         Map = map;
      }

      public MetaColumnToken(MultiPartAliasedToken other, ColumnMap map, ColumnMeta meta) : base(other)
      {
         Meta = meta;
         Map = map;
      }

      public ColumnMap Map { get; private set; }
      public ColumnMeta Meta { get; private set; }
   }
}