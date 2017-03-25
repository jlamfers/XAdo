using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class ColumnPartial : MultiPartAliasedPartial, ICloneable
   {
      // temp help field, set after parsing
      internal string Path;

      private static readonly char[] OperatorChars = { '(', '+', '-', '*', '/', '~', '&', '|' };

      private ColumnPartial()
      {
         Index = -1;
      }

      public ColumnPartial(IList<string> parts, string alias, string tag, ColumnMap map = null, int index = -1)
         : base(string.Join(".",parts))
      {
         RawAlias = alias;
         RawParts = parts.ToList().AsReadOnly();
         alias = alias.UnquotePartial();
         Tag = tag != null ? tag.Trim() : null;
         Alias = alias;
         Parts = parts.Select(p => p.UnquotePartial()).ToList().AsReadOnly();

         Path = Alias ?? Parts.LastOrDefault();
         // true if not quoted and any operator chars found
         IsCalculated = Parts.SequenceEqual(RawParts) && Parts.Any(p => p.IndexOfAny(OperatorChars) != -1);

         string mapFromTag;
         Meta = new ColumnMeta().InitializeByTag(Tag,IsCalculated, out mapFromTag);
         if (mapFromTag != null)
         {
            Path = mapFromTag;
         }

         Map = map; // optional, if not set, it is later determined by path setting
         Index = index;
         
      }

      public ColumnMeta Meta { get;private set; }
      public ColumnMap Map { get; private set; }

      public bool CanUpdate()
      {
         return Meta != null && Meta.Persistency.CanUpdate();
      }
      public bool CanCreate()
      {
         return Meta != null && Meta.Persistency.CanCreate();
      }
      public bool CanDelete()
      {
         return Meta != null && Meta.Persistency.CanDelete();
      }

      public string Tag { get; private set; }

      public bool IsCalculated { get; private set; }

      public int Index { get; private set; }

      public string Schema
      {
         get { return Parts.Count >= 3 ? Parts[0] : null; }
      }
      public string TableName
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

      public TablePartial Table { get; private set; }

      internal void SetAlias(string alias)
      {
         RawAlias = Alias = alias;
      }
      internal void SetMap(ColumnMap map)
      {
         Map = map;
      }
      internal void SetTable(TablePartial table, bool aliasChanged = false)
      {
         if (Table != null && !ReferenceEquals(Table, table))
         {
            throw new QuobException("column table was already set to another table. You cannot switch column tables.");
         }
         if (IsCalculated)
         {
            throw new QuobException("You cannot set the table for any calculated column.");
         }
         Table = table;
         if (aliasChanged)
         {
            var parts = Parts.ToList();
            var rawParts = RawParts.ToList();
            if (Schema != null)
            {
               parts.RemoveAt(0);
               rawParts.RemoveAt(0);
            }
            if (TableName == null)
            {
               if (table.Alias != null)
               {
                  parts.Insert(0, table.Alias);
                  rawParts.Insert(0, table.Alias);
               }
               else
               {
                  parts.Insert(0, table.TableName);
                  rawParts.Insert(0, table.RawParts.Last());
               }
            }
            else
            {
               if (table.Alias != null)
               {
                  parts[0] = table.Alias;
                  rawParts[0] = table.Alias;
               }
               else
               {
                  parts[0] = table.TableName;
                  rawParts[0] = table.RawParts.Last();
               }
            }
            Parts = parts.AsReadOnly();
            RawParts = rawParts.AsReadOnly();
            Expression = string.Join(".", RawParts);
         }
      }
      internal void SetIndex(int index)
      {
         Index = index;
      }

      internal bool SameColumn(ColumnPartial other)
      {
         return other.Schema == Schema && other.TableName == TableName && other.ColumnName == ColumnName;
      }

      object ICloneable.Clone()
      {
         return Clone();
      }
      public ColumnPartial Clone()
      {
         //Table is NOT cloned, Path is not cloned
         return new ColumnPartial{Expression = Expression,Alias = Alias,Parts=Parts.ToList().AsReadOnly(),RawAlias = RawAlias,RawParts = RawParts.ToList().AsReadOnly(), Tag = Tag,IsCalculated = IsCalculated, Meta = Meta.Clone(), Map = Map, Index = Index};
      }
     
   }

}