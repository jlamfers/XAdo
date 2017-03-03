using System.Collections.Generic;
using XAdo.Core;
using XAdo.Quobs.Core.Mapper;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class MetaColumnPartial : ColumnPartial
   {
      public MetaColumnPartial(IList<string> parts, string alias, ColumnMap map, ColumnMeta meta, int  index) : base(parts, alias)
      {
         Meta = meta;
         Map = map;
         Index = index;
      }

      public MetaColumnPartial(MultiPartAliasedPartial other, ColumnMap map, ColumnMeta meta, int index) : base(other)
      {
         Meta = meta;
         Map = map;
         Index = index;
      }

      public ColumnMap Map { get; private set; }
      public ColumnMeta Meta { get; private set; }
      public TablePartial Table { get; internal set; }// to be resolved by QueryBuilder
      public AdoColumnMeta AdoMeta { get; internal set; }// to be resolved by QueryBuilder
      public int Index { get; private set; }

      public virtual MetaColumnPartial Clone()
      {
         return new MetaColumnPartial(this, Map, Meta, Index){Table = Table};
      }
   }
}