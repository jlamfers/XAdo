using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using XAdo.Sql.Core.Parser;

namespace XAdo.Sql.Core
{

   public class SqlSelectInfo
   {
      private string _innerQuery;

      private SqlSelectInfo() { }

      public SqlSelectInfo(string sql, IList<ColumnInfo> columns, IList<TableInfo> tables, bool distinct, int selectColumnsPosition, int fromPosition, int wherePosition)
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
         Tables = tables.ToList().AsReadOnly();
         Distinct = distinct;
         SelectColumnsPosition = selectColumnsPosition;
         FromPosition = newFromPosition;
         WherePosition = wherePosition - (fromPosition - newFromPosition);

      }

      public string Sql { get; private set; }
      public IList<ColumnInfo> Columns { get; private set; }
      public IList<TableInfo> Tables { get; private set; }
      public bool Distinct { get; private set; }
      public int SelectColumnsPosition { get; private set; }
      public int FromPosition { get; private set; }
      public int WherePosition { get; private set; }

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
               + (Distinct ? /* ensure each column to have unique aliases*/ string.Join(",", Columns.Select(c => c.Expression + " AS " + ("c" + index++)).ToArray()) : "1 AS c0")
               + " "
               + Sql.Substring(FromPosition);
         }
         return _innerQuery;
      }

      //public string AsUpdateQuery()
      //{
      //   foreach (var table in Tables)
      //   {
      //      var table1 = table;
      //      var columns = Columns.Where(c => (c.Table ?? Tables.First()).BelongsTo(table1)).ToList();
      //      var updateableColumns = columns.Where(c => c.Persistency.HasFlag(PersistencyType.Update));
      //      if (!updateableColumns.Any())
      //      {
      //         // cannot update
      //         continue;
      //      }
      //      var keyColumns = columns.Where(c => c.IsKey).ToList();
      //      if (!keyColumns.Any())
      //      {
      //         keyColumns = Columns.Where(c => c.IsKey).ToList();
      //      }
      //      if (!keyColumns.Any())
      //      {
      //         // cannot update
      //         continue;
      //      }
      //   }
      //}

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
         var divPosition = FromPosition - result.Sql.Length;
         result.FromPosition = result.Sql.Length;
         result.WherePosition = WherePosition - divPosition;
         result.Sql += Sql.Substring(FromPosition);
         return result;
      }

      public SqlSelectInfo Map(IList<string> columns)
      {
         return Map(columns.Select(c => FindColumn(c).Clone()).ToList());
      }

   }
}