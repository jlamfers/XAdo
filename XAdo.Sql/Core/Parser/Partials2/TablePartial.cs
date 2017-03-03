using System;
using System.Collections.Generic;
using System.Linq;
using XAdo.Quobs.Core.Common;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public sealed class TablePartial : MultiPartAliasedPartial, ICloneable
   {
      private TablePartial() { }

      public TablePartial(IList<string> parts, string alias)
      {
         RawAlias = alias;
         RawParts = parts.ToList().AsReadOnly();

         Alias = alias.UnquotePartial();
         Parts = parts.Select(p => p.UnquotePartial()).ToList().AsReadOnly();

      }

      public string Schema
      {
         get { return Parts.Count >= 2 ? Parts[Parts.Count - 2] : null; }
      }
      public string TableName
      {
         get { return Parts[Parts.Count - 1]; }
      }

      public string NameOrAlias
      {
         get { return !string.IsNullOrEmpty(Alias) ? Alias : TableName; }
      }

      public IList<ColumnPartial> Columns { get; private set; }

      internal void SetAlias(string alias)
      {
         RawAlias = Alias = alias;
         if (Columns == null) return;
         foreach (var c in Columns)
         {
            c.SetTable(this,true);
         }
      }
      internal void AttchOwnedColumns(IEnumerable<ColumnPartial> columns)
      {
         Columns = columns.Where(IsColumnOwnerOf).ToList().AsReadOnly();
      }


      object ICloneable.Clone()
      {
         return Clone();
      }

      public TablePartial Clone()
      {
         // columns are NOT cloned
         return new TablePartial { Expression = Expression, Alias = Alias, Parts = Parts, RawAlias = RawAlias, RawParts = RawParts};
      }

      internal bool SameTable(TablePartial other)
      {
         return other.Schema == Schema && other.TableName == TableName;
      }

      private bool IsColumnOwnerOf(ColumnPartial column)
      {
         if (column.IsCalculated)
         {
            // readonly collumn
            return false;
         }
         if (Alias != null)
         {
            return column.Schema == null && column.TableName.EqualsOrdinalIgnoreCase(Alias);
         }
         if (Schema != null && column.Schema != null)
         {
            return column.Schema.EqualsOrdinalIgnoreCase(Schema) && column.TableName.EqualsOrdinalIgnoreCase(TableName);
         }
         return column.TableName.EqualsOrdinalIgnoreCase(TableName);
      }

   }
}