﻿using System.Collections;
using System.Linq;
using System.Text;
using Sql.Parser.Tokens;

namespace Sql.Parser
{
   public class ColumnMeta
   {

      private PersistencyType? _persistencyType;

      internal ColumnMeta()
      {
      }

      public bool IsKey { get; private set; }
      public bool IsAutoIncrement { get; private set; }
      public bool IsCalculated { get; private set; }
      public bool NotNull { get; private set; }
      public bool IsOuterJoinColumn { get; private set; }
      public PersistencyType Persistency
      {
         get { return _persistencyType.GetValueOrDefault(PersistencyType.Default); }
         private set { _persistencyType = value; }
      }

      public static ColumnMeta FindMeta(TagToken token, bool distinct, out string relativeName)
      {
         relativeName = token.Expression;
         return FindMeta(ref relativeName,distinct,false) ?? new ColumnMeta();
      }
      public static ColumnMeta FindMeta(ColumnToken token, bool distinct, out string relativeName)
      {
         var @readonly = token.Expression.Contains("(");
         var alias = token.Alias;
         var map = FindMeta(ref alias, distinct,@readonly);
         if (map != null)
         {
            alias = alias.TrimQuotes();
            if (string.IsNullOrWhiteSpace(alias))
            {
               alias = null;
            }
            token.Alias = alias;
            relativeName = alias ?? token.Parts.Last();
            return map;
         }
         var parts = token.Parts.ToList();
         var part = parts.Last();
         map = FindMeta(ref part, distinct, @readonly);
         relativeName = part;
         if (map != null)
         {
            part = part.TrimQuotes();
            parts[parts.Count - 1] = part;
            token.Parts = parts.AsReadOnly();
            relativeName = part;
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
            return null;
         }

         // optional seperator so that 'normal' characters (behind the seperator) can be interpreted as tags
         var sepIndex = carrier.IndexOf(':');
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
               case '*':
                  meta.IsKey = true;
                  meta.Persistency &= ~PersistencyType.Update;
                  break;
               case '@':
                  meta.IsCalculated = true;
                  meta.Persistency &= ~PersistencyType.Create;
                  meta.Persistency &= ~PersistencyType.Update;
                  break;
               case '+':
                  meta.IsAutoIncrement = true;
                  meta.IsKey = true;
                  meta.Persistency &= ~PersistencyType.Create;
                  meta.Persistency &= ~PersistencyType.Update;
                  break;
               case '?':
                  meta.IsOuterJoinColumn = true;
                  break;
               case '!':
                  meta.NotNull = true;
                  break;
               default:
                  if (sepIndex != -1 && i >= sepIndex)
                  {
                     switch (ch)
                     {
                        case ':':
                           break;
                        case 'c':
                        case 'C':
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Create;
                           break;
                        case 'u':
                        case 'U':
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Update;
                           break;
                        case 'r':
                        case 'R':
                           meta.Persistency = meta._persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Read;
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
         if (IsKey) sb.Append("*");
         if (IsAutoIncrement) sb.Append("+");
         if (IsCalculated) sb.Append("@");
         if (NotNull) sb.Append("!");
         if (IsOuterJoinColumn) sb.Append("?");
         sb.Append(":");
         sb.Append(Persistency.HasFlag(PersistencyType.Create) ? "C" : "-");
         sb.Append(Persistency.HasFlag(PersistencyType.Read) ? "R" : "-");
         sb.Append(Persistency.HasFlag(PersistencyType.Update) ? "U" : "-");
         return sb.ToString();
      }
   }
}
