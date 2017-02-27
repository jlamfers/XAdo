using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace XAdo.Quobs.Linq
{
   public interface IUrlExpressionParser
   {
      Expression Parse(string filter, Type inputType, Type resultType);
      ICollection<char> ColumnSepChars { get; }
      ICollection<char> AliasSepChars { get; }
   }

   public static class UrlExpressionParserExtensions
   {
      public static IList<Tuple<string, string>> SplitColumns(this IUrlExpressionParser self, string expression)
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

      private static string Trim(this StringBuilder self)
      {
         var expression = self.ToString();
         while (expression.Length > 1 && expression[0] == '(' && expression.Last() == ')')
         {
            expression = expression.Substring(1, expression.Length - 2).Trim();
         }
         return expression.Trim();

      }
      
   }
}
