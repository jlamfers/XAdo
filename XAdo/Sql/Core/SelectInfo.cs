using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XAdo.Sql.Core
{

   public class SelectInfo
   {
      private string _innerQuery;

      public SelectInfo(string sql, IList<ColumnInfo> columns, IDictionary<string, string> tables, bool distinct, int selectColumnsPosition, int fromPosition)
      {
         for (var index = 0; index < columns.Count; index++)
         {
            var column = columns[index];
            var prevPath = index == 0 ? "" : columns[index - 1].Path;
            column.ResolveFullPath(prevPath);
            column.Index = index;
         }

         // get rid of tags...
         Sql = sql.Substring(0, selectColumnsPosition)
               + Environment.NewLine
               + "   "+string.Join("," + Environment.NewLine+"   ", columns.Select(c => c.Expression + (c.Alias != null ? " AS " + c.Alias : "")))
               + Environment.NewLine;

         var newFromPosition = Sql.Length;

         Sql += sql.Substring(fromPosition);

         Columns = columns.ToList().AsReadOnly();
         Tables = new ReadOnlyDictionary<string, string>(tables);
         Distinct = distinct;
         SelectColumnsPosition = selectColumnsPosition;
         FromPosition = newFromPosition;

      }

      public string Sql { get; private set; }
      public IList<ColumnInfo> Columns { get; private set; }
      public IDictionary<string,string> Tables { get; private set; }
      public bool Distinct { get; private set; }
      public int SelectColumnsPosition { get; private set; }
      public int FromPosition { get; private set; }

      public string AsInnerQuery()
      {
         if (_innerQuery == null)
         {
            var index = 0;
            _innerQuery =
               Sql.Substring(0, SelectColumnsPosition)
               + " "
               + (Distinct ? string.Join(",", Columns.Select(c => c.Expression + " AS " + ("c" + index++)).ToArray()) : "1 AS c0")
               + " "
               + Sql.Substring(FromPosition);
         }
         return _innerQuery;
      }
   }
}