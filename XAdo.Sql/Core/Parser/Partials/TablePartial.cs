using System;
using System.Collections.Generic;
using System.Linq;
using XAdo.Core.SimpleJson;
using XAdo.DbSchema;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class TablePartial : MultiPartAliasedPartial, ICloneable
   {
      private string _tag;
      private TablePartial() { }

      public TablePartial(IList<string> parts, string alias, string tag)
         : base(string.Join(".",parts))
      {
         Tag = tag;

         RawAlias = alias;
         RawParts = parts.ToList().AsReadOnly();

         Alias = alias.UnquotePartial();
         Parts = parts.Select(p => p.UnquotePartial()).ToList().AsReadOnly();

      }

      public string Tag
      {
         get { return _tag; }
         internal set
         {
            _tag = value;
            Persistency = _tag != null
               ? (PersistencyType?) SimpleJson.DeserializeObject<JsonAnnotation>(_tag).crud.ToPersistencyType()
               : null;
         }
      }

      public PersistencyType? Persistency { get; private set; }

      public bool CanUpdate()
      {
         return Persistency.CanUpdate();
      }
      public bool CanCreate()
      {
         return Persistency.CanCreate();
      }
      public bool CanDelete()
      {
         return Persistency.CanDelete();
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
      public DbTableItem DbTable { get; internal set; }

      internal void SetAlias(string alias)
      {
         RawAlias = Alias = alias;
         if (Columns == null) return;
         foreach (var c in Columns)
         {
            c.SetTable(this,true);
         }
      }
      internal void AttachOwnedColumns(IEnumerable<ColumnPartial> columns)
      {
         Columns = columns.Where(IsColumnOwnerOf).ToList().AsReadOnly();
      }
      internal void AttachColumns(IEnumerable<ColumnPartial> columns)
      {
         Columns = columns.ToList().AsReadOnly();
         foreach (var c in Columns)
         {
            c.SetTable(this);
         }
      }


      object ICloneable.Clone()
      {
         return Clone();
      }

      public TablePartial Clone()
      {
         // columns are NOT cloned
         return new TablePartial { Expression = Expression, Alias = Alias, Parts = Parts.ToList().AsReadOnly(), RawAlias = RawAlias, RawParts = RawParts.ToList().AsReadOnly(), Tag = Tag, DbTable = DbTable, Persistency = Persistency};
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