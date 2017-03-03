using System;
using System.Linq;
using XAdo.Quobs.Core.Parser;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public class ColumnMap
   {

      public ColumnMap(string fullname)
      {
         if (fullname == null)
         {
            return;
         }
         if (fullname == null) throw new ArgumentNullException("fullname");
         FullName = fullname;
         var parts = FullName.Split(Constants.Syntax.Chars.COLUMN_SEP);
         Name = parts.Last();
         Path = parts.Length == 1 ? "" : string.Join(Constants.Syntax.Chars.COLUMN_SEP_STR, parts, 0, parts.Length - 1);
      }

      public string Path { get; private set; }
      public string Name { get; private set; }
      public string FullName { get; private set; }

      public override string ToString()
      {
         return FullName;
      }
   }
}