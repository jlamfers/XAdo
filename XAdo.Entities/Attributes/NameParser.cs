using System.Collections.Generic;
using System.Text;

namespace XAdo.Quobs.Attributes
{
   internal static class NameParser
   {
      public static List<string> FindParts(string name)
      {
         if (name == null)
         {
            return null;
         }
         var parts = new List<string>();
         var word = new StringBuilder();
         var escaped = false;
         foreach (var ch in name)
         {
            if (escaped)
            {
               escaped = false;
               word.Append(ch);
               continue;
            }
            switch (ch)
            {
               case'.':
                  parts.Add(word.ToString());
                  word.Length = 0;
                  break;
               case '\\':
                  escaped = true;
                  break;
               default:
                  word.Append(ch);
                  break;
            }
         }
         if (word.Length > 0)
         {
            parts.Add(word.ToString());
         }

         return parts;
      }
   }
}
