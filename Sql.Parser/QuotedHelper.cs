using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sql.Parser
{
   public static class QuotedHelper
   {
      public static readonly IDictionary<char, char> Quotes =
         new ReadOnlyDictionary<char, char>(new Dictionary<char, char>
         {
            {'\'','\''},
            {'"','"'},
            {'`','`'},
            {'[',']'}
         });

      public static List<string> SplitMultiPartIdentifier(this string self)
      {
         var parts = new List<string>();
         var part = new StringBuilder();
         for (var i = 0; i < self.Length; i++)
         {
            var ch = self[i];
            if (Quotes.ContainsKey(ch))
            {
               self.ReadQuoted(ref i);
               parts.Add(part.ToString());
               part.Length = 0;
               continue;
            }
            switch (ch)
            {
               case ' ':
               case '.':
                  if (part.Length > 0)
                  {
                     parts.Add(part.ToString());
                  }
                  part.Length = 0;
                  break;
               default:
                  part.Append(ch);
                  break;
            }
         }
         parts.Add(part.ToString());
         return parts;
      }

      public static string TrimQuotes(this string self)
      {
         if (string.IsNullOrEmpty(self)) return self;
         var left = self[0];
         char right;
         if (Quotes.TryGetValue(left, out right) && self.Last() == right)
         {
            return self.Substring(1, self.Length - 2);
         }
         return self;
      }

      public static string ReadQuoted(this string self, ref int index)
      {
         if (index >= self.Length - 1)
         {
            throw new SqlParserException(self, index, "Unexpected EOF");
         }
         var left = self[index];
         char right;
         if (!Quotes.TryGetValue(left, out right))
         {
            throw new SqlParserException(self,index,"Quote character expected");
         }
         var sb = new StringBuilder();
         for (var i = index; i < self.Length; i++)
         {
            char ch;
            sb.Append(ch = self[i]);
            if (ch == right)
            {
               index = i;
               return sb.ToString();
            }
         }
         throw new SqlParserException(self,index, "Unterminated quoted");
      }
   }
}
