using System;
using System.Linq;
using XAdo.Quobs.Core.Impl;

namespace XAdo.Quobs.Core.Parser
{
   public static class Extensions
   {
      public static string UnquotePartial(this string self)
      {
         if (String.IsNullOrEmpty(self)) return self;
         var left = self[0];
         char right;
         if (SqlScannerImpl.Quotes.TryGetValue(left, out right) && self.Last() == right)
         {
            if (self.IndexOf(right) == self.Length - 1)
            {
               // not any end quote in between?
               return self.Substring(1, self.Length - 2);
            }
            var test = self.Replace(new string(right, 2), "");
            if (test.IndexOf(right) == test.Length - 1)
            {
               // any end quote in between is escaped?
               return self.Substring(1, self.Length - 2);
            }
         }
         return self;
      }

      public static bool IsQuotedPartial(this string self)
      {
         if (String.IsNullOrEmpty(self)) return false;
         var left = self[0];
         char right;
         if (SqlScannerImpl.Quotes.TryGetValue(left, out right) && self.Last() == right)
         {
            if (self.IndexOf(right) == self.Length - 1)
            {
               // not any end quote in between?
               return true;
            }
            var test = self.Replace(new string(right, 2), "");
            if (test.IndexOf(right) == test.Length - 1)
            {
               // any end quote in between is escaped?
               return true;
            }
         }
         return false;
      }
   }
}
