using System;
using System.Collections.Generic;
using System.Text;

namespace XAdo.Sql.Core.Parser
{
   public static class MultiPartSplitter
   {
      public static List<string> SplitMultiPartIdentifier(this string self)
      {
         var parts = new List<string>();
         var part = new StringBuilder();
         for (var i = 0; i < self.Length; i++)
         {
            var ch = self[i];
            switch (ch)
            {
               case '\'':
               case '"':
               case '[':
               case '`':
                  self.ReadQuoted(ch, part, ref i);
                  parts.Add(part.ToString());
                  part.Length = 0;
                  break;
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

      public static string UnQuote(this string self)
      {
         if (string.IsNullOrEmpty(self)) return self;
         switch (self[0])
         {
            case '\'':
            case '"':
            case '[':
            case '`':
               return self.Substring(1, self.Length - 2);
         }
         return self;
      }

      private static void ReadQuoted(this string self, char left, StringBuilder sb, ref int index)
      {
         char right = left == '[' ? ']' : left;
         for (var i = index; i < self.Length; i++)
         {
            char ch;
            sb.Append(ch = self[i]);
            if (ch == right)
            {
               index = i;
               return;
            }
         }
         throw new Exception("Unterminated quoted");
      }
   }
}
