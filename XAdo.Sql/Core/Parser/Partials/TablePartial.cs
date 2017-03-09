using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class TablePartial : MultiPartAliasedPartial, ICloneable
   {
      private TablePartial() { }

      public TablePartial(IList<string> parts, string alias, string tag)
         : base(string.Join(Constants.Syntax.Chars.COLUMN_SEP_STR,parts))
      {
         Tag = tag;

         RawAlias = alias;
         RawParts = parts.ToList().AsReadOnly();

         Alias = alias.UnquotePartial();
         Parts = parts.Select(p => p.UnquotePartial()).ToList().AsReadOnly();

      }

      public string Tag { get; internal set; }

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
      internal void AttchColumns(IEnumerable<ColumnPartial> columns)
      {
         Columns = columns.ToList().AsReadOnly();
      }


      object ICloneable.Clone()
      {
         return Clone();
      }

      public TablePartial Clone()
      {
         // columns are NOT cloned
         return new TablePartial { Expression = Expression, Alias = Alias, Parts = Parts.ToList().AsReadOnly(), RawAlias = RawAlias, RawParts = RawParts.ToList().AsReadOnly(), Tag = Tag};
      }

      internal bool SameTable(TablePartial other)
      {
         return other.Schema == Schema && other.TableName == TableName;
      }

      internal bool IsColumnOwnerOf(ColumnPartial column)
      {
         if (column.IsCalculated)
         {
            // readonly collumn
            return false;
         }
         bool result;
         if (Alias != null)
         {
            result = column.Schema == null && column.TableName.EqualsOrdinalIgnoreCase(Alias);
         }
         else if (Schema != null && column.Schema != null)
         {
            result = column.Schema.EqualsOrdinalIgnoreCase(Schema) &&
                     column.TableName.EqualsOrdinalIgnoreCase(TableName);
         }
         else
         {
            result = column.TableName.EqualsOrdinalIgnoreCase(TableName);
         }
         if (result && column.Table == null)
         {
            column.SetTable(this);
         }
         return result;
      }

   }
}