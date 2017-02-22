using System;
using System.Linq;

namespace Sql.Parser.Parser
{
   public static class Extensions
   {
      public static string TrimQuotes(this string self)
      {
         if (String.IsNullOrEmpty(self)) return self;
         var left = self[0];
         char right;
         if (Scanner.Quotes.TryGetValue(left, out right) && self.Last() == right)
         {
            return self.Substring(1, self.Length - 2);
         }
         return self;
      }

      public static bool IsQuoted(this string self)
      {
         if (String.IsNullOrEmpty(self)) return false;
         var left = self[0];
         char right;
         return Scanner.Quotes.TryGetValue(left, out right) && self.Last() == right;
      }
   }
}
