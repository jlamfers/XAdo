using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Quobs.Dialect
{
   public class SqlServer2012Formatter : SqlServerFormatter
   {
      private class SqlServer2012Dialect : SqlServerDialect
      {
         public override string Concat
         {
            get { return "CONCAT({0,...})"; }
         }
      }

      public SqlServer2012Formatter()
         : base(new SqlServer2012Dialect())
      {
         
      }

      protected override void WritePagedQuery(TextWriter writer, string sqlSelectWithoutOrder, IEnumerable<string> orderByClause, IEnumerable<string> selectNames, string skip, string take)
      {
         var count = selectNames.FirstOrDefault() == "COUNT(1)";
         if (count)
         {
            writer.WriteLine("SELECT COUNT(1) FROM (");
         }
         skip = skip ?? "0";
         take = take ?? ("" + (int.MaxValue - 10));
         writer.WriteLine(sqlSelectWithoutOrder);
         if (orderByClause != null && orderByClause.Any())
         {
            writer.Write("ORDER BY ");
            writer.WriteLine(string.Join(", ", orderByClause.ToArray()));
         }
         writer.WriteLine("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
         if (count)
         {
            writer.Write(") __pt_inner");
         }
      }
   }
}