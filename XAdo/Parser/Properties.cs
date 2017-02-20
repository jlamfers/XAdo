using System.Collections;
using System.Collections.Generic;

namespace XAdo.Parser
{
   public class SelectProperties
   {
      public SelectProperties(bool distinct, int top)
      {
         Distinct = distinct;
         Top = top;
      }

      public bool Distinct { get; private set; }
      public int Top { get; private set; }
   }

   public class TableInfo
   {
      public TableInfo(string expression, string alias)
      {
         //Raw = raw;
      }
      public string Raw { get;private set; }
      public string Alias { get; private set; }
      public string Name { get; private set; }
      public string Schema { get; private set; }
   }

   public class MapInfo
   {
      public string Path { get; private set; }
      public string Name { get; private set; }
      public string FullName { get; private set; }
   }

   public class MetaInfo
   {
      public MetaInfo(string raw)
      {
         Raw = raw;
      }

      public string Raw { get; private set; }

      public MapInfo Map { get; private set; }
      public bool IsKey { get; private set; }
      public bool IsCalculated { get; private set; }
   }

   public class ColumnInfo
   {
      public ColumnInfo(string expression, string alias, string map)
      {
      }
      public string Raw { get;private set; }
      public string Alias { get; private set; }
      public string Expression { get; private set; }
      public string ColumnName { get; private set; }
      public MetaInfo Meta { get; private set; }
      public TableInfo Table { get; private set; }
   }

   public class JoinInfo
   {
      public TableInfo Left { get; private set; }
      public TableInfo Right { get; private set; }

   }

   public class SelectInfo
   {

      public string Sql { get; private set; }

      public int SelectColumnsIndex { get; private set; }
      public int FromIndex { get; private set; }
      public int WhereIndex { get; private set; }
      public IList<ColumnInfo> Columns { get;private set; }
      public IList<TableInfo> Tables { get; private set; }
   }
}
