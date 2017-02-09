using System.Collections.Generic;

namespace XAdo.Sql.Core
{
   public class SelectInfo
   {
      public SelectInfo(IList<ColumnInfo> columns, string tableName)
      {
         TableName = tableName;
         Columns = columns;
      }

      public IList<ColumnInfo> Columns { get; private set; }
      public string TableName { get; private set; }
   }
}