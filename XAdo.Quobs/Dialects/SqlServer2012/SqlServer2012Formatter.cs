using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Dialects.SqlServer;

namespace XAdo.Quobs.Dialects.SqlServer2012
{
   public class SqlServer2012Formatter : SqlServerFormatter
   {

      public SqlServer2012Formatter()
         : base(new SqlServer2012Dialect())
      {
         
      }

      protected override void WritePagedQuery(TextWriter writer, string sqlSelectWithoutOrder, IEnumerable<string> orderByClause, IEnumerable<string> selectNames, string skip, string take)
      {
         if (skip == null && take == null)
         {
            writer.WriteLine(sqlSelectWithoutOrder.TrimEnd());
            writer.Write("   ORDER BY ");
            writer.Write(string.Join(", ", orderByClause.ToArray()));
            return;
         }

         if (orderByClause == null || !orderByClause.Any())
         {
            throw new QuobException("For SQL paging at least one order column must be specified.");
         }

         var count = selectNames.FirstOrDefault() == "COUNT(1)";
         if (count)
         {
            writer.WriteLine("SELECT COUNT(1) FROM (");
         }
         skip = skip ?? "0";
         take = take ?? ("" + (int.MaxValue - 10));
         writer.WriteLine(sqlSelectWithoutOrder);
         writer.Write("ORDER BY ");
         writer.WriteLine(string.Join(", ", orderByClause.ToArray()));
         writer.WriteLine("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
         if (count)
         {
            writer.Write(") __pt_inner");
         }
      }
   }
}