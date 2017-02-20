using System;
using System.Linq;

namespace Sql.Parser
{
   public class ColumnMap
   {

      public ColumnMap(string fullname)
      {
         if (fullname == null) throw new ArgumentNullException("fullname");
         FullName = fullname;
         var parts = FullName.Split('.');
         Name = parts.Last();
         Path = parts.Length == 1 ? "" : string.Join(".", parts, 0, parts.Length - 1);
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