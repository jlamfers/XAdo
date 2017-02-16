using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XAdo.Sql.Core
{

   public class SqlSelectInfo
   {
      private string _innerQuery;

      private SqlSelectInfo() { }

      public SqlSelectInfo(string sql, IList<ColumnInfo> columns, IDictionary<string, string> tables, bool distinct, int selectColumnsPosition, int fromPosition)
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

      private Dictionary<string, ColumnInfo>
         _fullNameMap;
      public ColumnInfo FindColumn(string fullname)
      {
         _fullNameMap = _fullNameMap ?? Columns.ToDictionary(c => c.FullName, c => c, StringComparer.OrdinalIgnoreCase);
         ColumnInfo column;
         _fullNameMap.TryGetValue(fullname, out column);
         return column;
      }

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

      public SqlSelectInfo Map(List<ColumnInfo> columns)
      {
         var result = new SqlSelectInfo
         {
            Columns = columns.Select((c, i) =>
            {
               c.Index = i;
               return c;
            }).ToList().AsReadOnly(),
            Tables = Tables,
            Distinct = Distinct,
            SelectColumnsPosition = SelectColumnsPosition,
            Sql = Sql.Substring(0, SelectColumnsPosition)
                  + Environment.NewLine
                  + "   " +
                  string.Join("," + Environment.NewLine + "   ",
                     columns.Select(c => c.Expression + (c.Alias != null ? " AS " + c.Alias : "")))
                  + Environment.NewLine
         };

         result.FromPosition = result.Sql.Length;
         result.Sql += Sql.Substring(FromPosition);
         return result;
      }

      public SqlSelectInfo Map(IList<string> columns)
      {
         return Map(columns.Select(c => FindColumn(c).Clone()).ToList());
      }

   }
}