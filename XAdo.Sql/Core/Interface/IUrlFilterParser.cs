using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace XAdo.Quobs.Core.Interface
{
   public interface IUrlFilterParser
   {
      Expression ParseExpression(string filter, Type inputType, Type resultType);
      ICollection<char> ColumnSepChars { get; }
      ICollection<char> AliasSepChars { get; }
   }

   public static class UrlFilterParserExtensions
   {
      public static IList<Tuple<string, string>> SplitColumns(this IUrlFilterParser self, string expression)
      {
         if (expression == null) return null;
         var result = new List<Tuple<string, string>>();
         var column = new StringBuilder();
         var alias = new StringBuilder();
         var current = column;
         var readingString = false;
         var parenCount = 0;
         for (var i = 0; i < expression.Length; i++)
         {
            var ch = expression[i];
            switch (ch)
            {
               case '(':
                  current.Append(ch);
                  if (!readingString) parenCount++;
                  continue;
               case ')':
                  current.Append(ch);
                  if (!readingString) parenCount--;
                  continue;
               case '\'':
                  current.Append(ch);
                  if (readingString && i < expression.Length - 1 && expression[i + 1] == '\'')
                  {
                     current.Append(ch);
                     i++;
                  }
                  else
                  {
                     readingString = !readingString;
                  }
                  continue;
            }
            if (parenCount > 0 || readingString)
            {
               current.Append(ch);
               continue;
            }

            if (self.AliasSepChars.Contains(ch))
            {
               current = alias;
               continue;
            }

            if (self.ColumnSepChars.Contains(ch))
            {
               if (column.Length > 0)
               {
                  result.Add(Tuple.Create(column.Trim(), alias.Trim()));
               }
               current = column;
               column.Length = 0;
               alias.Length = 0;
               continue;
            }
            current.Append(ch);
         }
         if (column.Length > 0)
         {
            result.Add(Tuple.Create(column.Trim(), alias.Trim()));
         }
         return result;
      }

      // e.g., split: select(id~name).where(name~ct~J).order(-name~id).page(1-10)
      public static IList<Tuple<string, string>> SplitQuery(this string query)
      {
         var method = new StringBuilder();
         var expression = new StringBuilder();
         var current = method;
         var quoted = false;
         var parenDepth = 0;

         var tokens = new List<Tuple<string, string>>();
         for (var i = 0; i < query.Length; i++)
         {
            var ch = query[i];
            switch (ch)
            {
               case '\'':
                  if (!quoted)
                  {
                     quoted = true;
                  }
                  else
                  {
                     if (i == query.Length - 1 || query[i + 1] != '\'')
                     {
                        quoted = false;
                     }
                  }
                  break;
               case '.':
                  if (quoted || parenDepth > 0)
                  {
                     break;
                  }
                  continue;
               case '(':
                  if (quoted)
                  {
                     break;
                  }
                  if (parenDepth++ == 0)
                  {
                     current = expression;
                     continue;
                  }
                  break;
               case ')':
                  if (quoted)
                  {
                     break;
                  }
                  if (--parenDepth == 0)
                  {
                     tokens.Add(Tuple.Create(method.ToString(), expression.ToString()));
                     method.Length = 0;
                     expression.Length = 0;
                     current = method;
                     continue;
                  }
                  break;

            }
            current.Append(ch);
         }

         return tokens;
      }

      private static string Trim(this StringBuilder self)
      {
         return self.ToString().Trim();
      }
      
   }
}
