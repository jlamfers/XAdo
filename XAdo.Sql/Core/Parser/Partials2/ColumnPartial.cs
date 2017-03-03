using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public class ColumnPartial : MultiPartAliasedPartial, ICloneable
   {
      // temp help field, set after parsing
      internal string Path;

      private static readonly char[] OperatorChars = { '(', '+', '-', '*', '/', '~', '&', '|' };

      protected ColumnPartial()
      {
         Index = -1;
      }

      public ColumnPartial(IList<string> parts, string alias, string tag)
      {
         RawAlias = alias;
         RawParts = parts.ToList().AsReadOnly();
         alias = alias.UnquotePartial();

         if (tag != null)
         {
            Tag = FindAnnotation(ref tag);
            Path = tag.Trim();
         }
         else
         {
            Tag = FindAnnotation(ref alias);
            Path = alias != null ? alias.Trim() : null;
         }

         Alias = alias;

         var partsList = parts.ToList();
         if (Tag == null)
         {
            var last = partsList.Last();
            Tag = FindAnnotation(ref last);
            Path = last.Trim();
            partsList[partsList.Count - 1] = last;
         }
         Parts = parts.Select(p => p.UnquotePartial()).ToList().AsReadOnly();

         // true if not quoted and any operator chars found
         IsCalculated = Parts.SequenceEqual(RawParts) && Parts.Any(p => p.IndexOfAny(OperatorChars) != -1);

         Meta = new ColumnMeta().InitializeByTag(Tag,IsCalculated);
      }

      public ColumnMeta Meta { get;protected set; }
      public ColumnMap Map { get; protected set; }

      public string Tag { get; protected set; }

      public bool IsCalculated { get; protected set; }

      public int Index { get; protected set; }

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

      public TablePartial Table { get; protected set; }

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
            throw new InvalidOperationException("column table was already set to another table. You cannot switch column tables.");
         }
         if (IsCalculated)
         {
            throw new InvalidOperationException("You cannot set the table for any calculated column.");
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

      private string FindAnnotation(ref string value)
      {
         if (value == null) return null;
         var tagIndex = value.IndexOfAny(Constants.Syntax.Chars.TagCharsSplitSet.ToArray());
         if (tagIndex == -1) return null;

         var tag = value.Substring(tagIndex);
         value = value.Substring(0, tagIndex);
         return tag;
      }


      object ICloneable.Clone()
      {
         return Clone();
      }
      public ColumnPartial Clone()
      {
         //Table is NOT cloned, Path is not cloned
         return new ColumnPartial{Expression = Expression,Alias = Alias,Parts=Parts,RawAlias = RawAlias,RawParts = RawParts, Tag = Tag,IsCalculated = IsCalculated, Meta = Meta, Map = Map, Index = Index};
      }
     
   }
}