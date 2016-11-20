using XAdo.Quobs.Core.SqlExpression.Sql;

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

      public static string Delimit(this ISqlFormatter formatter)
      {
         
      }

   }
}