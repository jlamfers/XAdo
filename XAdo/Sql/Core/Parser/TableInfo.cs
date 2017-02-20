using System;
using System.Linq;

namespace XAdo.Sql.Core.Parser
{
   public class TableInfo
   {
      public TableInfo(string raw)
      {
         Raw = raw;
         var parts = raw.SplitMultiPartIdentifier();
         if (parts.Count >= 3 && parts[parts.Count - 2].ToUpper() == "AS")
         {

            Alias = parts.Last().UnQuote();
            parts.RemoveRange(parts.Count-2,2);
         }
         parts.Reverse();
         Table = parts.First().UnQuote();
         Schema = parts.Skip(1).FirstOrDefault().UnQuote();
      }

      public string Raw { get; private set; }
      public string Alias { get; private set; }
      public string Table { get; private set; }
      public string Schema { get; private set; }

      public override string ToString()
      {
         return Raw;
      }

      public bool BelongsTo(TableInfo table)
      {
         var columnTable = this;
         if (columnTable.Schema != null)
         {
            return columnTable.Schema == table.Schema && columnTable.Table == table.Table;
         }
         return columnTable.Table == table.Alias || (table.Schema == null && table.Alias == null && columnTable.Table == table.Table);
      }
   }
}
