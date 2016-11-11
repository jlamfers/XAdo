using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.Core.DbSchema
{
   public static class Extensions
   {
      public static IList<DbSchemaDescriptor.JoinDescriptor> Sort(this IEnumerable<DbSchemaDescriptor.JoinDescriptor> self, Type startType)
      {
         var result = new List<DbSchemaDescriptor.JoinDescriptor>();
         var todo = new List<DbSchemaDescriptor.JoinDescriptor>();
         var list = new List<DbSchemaDescriptor.JoinDescriptor>(self);
         var count = list.Count;
         while (true)
         {
            foreach (var join in list)
            {
               if (join.LeftTableType == startType)
               {
                  result.Insert(0, join);
                  continue;
               }
               if (result.Any(x => x.RightTableType == join.LeftTableType))
               {
                  result.Add(join);
                  continue;
               }
               todo.Add(join);
            }
            if (todo.Count == 0)
            {
               return result;
            }
            if (todo.Count == count)
            {
               throw new ApplicationException("Joins cannot be chained");
            }
            count = todo.Count;
            list = todo;
            todo = new List<DbSchemaDescriptor.JoinDescriptor>();
         }
      }

      internal static string Delimit(this string self, string delimiterLeft, string delimiterRight)
      {
         if (self == null) return null;
         if (self.StartsWith(delimiterLeft)) return self;
         return delimiterLeft + self + delimiterRight;
      }
   }
}