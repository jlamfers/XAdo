using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Mapper;

namespace XAdo.Quobs.Core.Parser.Partials
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

      public bool OwnsColumn(ColumnPartial column)
      {
         var metaColumn = column as MetaColumnPartial;
         if (metaColumn != null && (metaColumn.Meta.IsCalculated || metaColumn.Meta.Persistency==PersistencyType.Read))
         {
            // readonly collumn
            return false;
         }
         if(Alias != null)
         {
            return column.Schema==null && column.TableName.EqualsOrdinalIgnoreCase(Alias);
         }
         if (Schema != null && column.Schema != null)
         {
            return column.Schema.EqualsOrdinalIgnoreCase(Schema) && column.TableName.EqualsOrdinalIgnoreCase(TableName);
         }
         return column.TableName.EqualsOrdinalIgnoreCase(TableName);
      }

   }
}