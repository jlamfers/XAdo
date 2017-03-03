using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Mapper
{
   public class ColumnMeta
   {

      private PersistencyType? _persistencyType;

      internal ColumnMeta(bool @readonly=false)
      {
         if (@readonly)
         {
            _persistencyType = PersistencyType.Read;
         }
      }

      public bool IsKey { get; private set; }
      public bool IsAutoIncrement { get; private set; }
      public bool IsCalculated { get; internal set; }
      public bool NotNull { get; private set; }
      public bool IsOuterJoinColumn { get; private set; }
      public PersistencyType Persistency
      {
         get { return _persistencyType.GetValueOrDefault(PersistencyType.Default); }
         internal set { _persistencyType = value; }
      }
      public Type Type { get; internal set; }

      internal static ColumnMeta FindMeta(TagPartial partial, bool distinct, out string relativeName)
      {
         relativeName = partial.Expression;
         return FindMeta(ref relativeName,distinct,false) ?? new ColumnMeta();
      }
      internal static ColumnMeta FindMeta(ColumnPartial partial, bool distinct, out string relativeName)
      {
         relativeName = null;
         var @readonly = partial.Expression.Contains("(");
         var alias = partial.Alias;
         var map = FindMeta(ref alias, distinct,@readonly);
         if (alias != null)
         {
            relativeName = alias;
         }
         if (map != null)
         {
            alias = alias.UnquotePartial();
            if (string.IsNullOrWhiteSpace(alias))
            {
               alias = null;
            }
            partial.SetAlias(alias);
            relativeName = alias ?? partial.Parts.Last();
            return map;
         }
         var parts = partial.Parts.ToList();
         var part = parts.Last();
         map = FindMeta(ref part, distinct, @readonly);
         relativeName = relativeName ?? part;
         if (map != null)
         {
            part = part.UnquotePartial();
            parts[parts.Count - 1] = part;
            partial.Parts = parts.AsReadOnly();

            var raw = partial.RawParts.ToList();
            var last = raw.Last();
            if (last.IsQuotedPartial())
            {
               raw[raw.Count - 1] = last[0] + part + last[last.Length - 1];
            }
            else
            {
               raw[raw.Count - 1] = part;
            }
            partial.RawParts = raw.AsReadOnly();

            relativeName = relativeName ?? part;
            return map;
         }
         if (distinct || @readonly)
         {
            return new ColumnMeta{_persistencyType = PersistencyType.Read};
         }
         return null;
      }
      private static ColumnMeta FindMeta(ref string carrier, bool distinct, bool @readonly)
      {
         if (string.IsNullOrWhiteSpace(carrier))
         {
            carrier = null;
            return null;
         }

         // optional seperator so that 'normal' characters (behind the seperator) can be interpreted as tags
         var starterIndex = carrier.IndexOf(Constants.Syntax.Chars.SPECIAL_CHARS_STARTER);
         var meta = new ColumnMeta();
         if (distinct || @readonly)
         {
            meta._persistencyType = PersistencyType.Read;
         }
         for (var i = carrier.Length - 1; i >= 0; i--)
         {
            var ch = carrier[i];
            switch (ch)
            {
               case '\r':
               case '\n':
               case '\t':
               case ' ':
                  continue;
               case Constants.Syntax.Chars.PRIMARY_KEY:
                  meta.IsKey = true;
                  meta.Persistency &= ~PersistencyType.Update;
                  break;
               case Constants.Syntax.Chars.CALCULATED:
                  meta.IsCalculated = true;
                  meta.Persistency &= ~PersistencyType.Create;
                  meta.Persistency &= ~PersistencyType.Update;
                  break;
               case Constants.Syntax.Chars.AUTO_INCREMENT:
                  meta.IsAutoIncrement = true;
                  meta.IsKey = true;
                  meta.Persistency &= ~PersistencyType.Create;
                  meta.Persistency &= ~PersistencyType.Update;
                  break;
               case Constants.Syntax.Chars.OUTER_JOIN_COLUMN:
                  meta.IsOuterJoinColumn = true;
                  break;
               case Constants.Syntax.Chars.NOT_NULL:
                  meta.NotNull = true;
                  break;
               default:
                  if (starterIndex != -1 && i >= starterIndex)
                  {
                     switch (char.ToUpper(ch))
                     {
                        case Constants.Syntax.Chars.SPECIAL_CHARS_STARTER:
                           break;
                        case Constants.Syntax.Chars.CREATE:
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Create;
                           break;
                        case Constants.Syntax.Chars.UPDATE:
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Update;
                           break;
                        case Constants.Syntax.Chars.READ:
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Read;
                           break;
                        case Constants.Syntax.Chars.DELETE:
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Delete;
                           break;
                     }
                  }
                  else
                  {
                     if (i == carrier.Length - 1)
                     {
                        meta = null;
                     }
                     carrier = i == carrier.Length - 1 ? carrier : carrier.Substring(0, i + 1);
                     return meta;
                  }
                  break;
            }
         }
         carrier = null;
         return meta;
      }

      public override string ToString()
      {
         var sb = new StringBuilder();
         if (IsKey) sb.Append(Constants.Syntax.Chars.PRIMARY_KEY);
         if (IsAutoIncrement) sb.Append(Constants.Syntax.Chars.AUTO_INCREMENT);
         if (IsCalculated) sb.Append(Constants.Syntax.Chars.CALCULATED);
         if (NotNull) sb.Append(Constants.Syntax.Chars.NOT_NULL);
         if (IsOuterJoinColumn) sb.Append(Constants.Syntax.Chars.OUTER_JOIN_COLUMN);
         sb.Append(Constants.Syntax.Chars.SPECIAL_CHARS_STARTER);
         sb.Append(Persistency.HasFlag(PersistencyType.Create) ? Constants.Syntax.Chars.CREATE : '-');
         sb.Append(Persistency.HasFlag(PersistencyType.Read) ? Constants.Syntax.Chars.READ : '-');
         sb.Append(Persistency.HasFlag(PersistencyType.Update) ? Constants.Syntax.Chars.UPDATE : '-');
         sb.Append(Persistency.HasFlag(PersistencyType.Delete) ? Constants.Syntax.Chars.DELETE : '-');
         return sb.ToString();
      }
   }
}
