using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.Core.DbSchema
{
   internal static class Extensions
   {
      public static string Delimit(this string self, string delimiterLeft, string delimiterRight)
      {
         if (self == null) return null;
         if (self.StartsWith(delimiterLeft)) return self;
         return delimiterLeft + self + delimiterRight;
      }
   }
}