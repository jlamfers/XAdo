using System;
using System.Collections.Generic;
using XAdo.Quobs.Sql.Formatter;

namespace XAdo.Quobs.Sql
{
   public class SqlSelectMeta : ISqlSelectMeta
   {
      public SqlSelectMeta()
      {
         GroupByColumns = new List<string>();
         OrderColumns = new List<string>();
         HavingClausePredicates = new List<string>();
         WhereClausePredicates = new List<string>();
         SelectColumns = new List<Tuple<string, string>>();
         Arguments = new Dictionary<string, object>();
      }

      public List<Tuple<string,string>> SelectColumns { get; private set; }
      public List<string> WhereClausePredicates { get; private set; }
      public List<string> HavingClausePredicates { get; private set; }
      public List<string> OrderColumns { get; private set; }
      public List<string> GroupByColumns { get; private set; }
      public string TableName { get; set; }
      public bool Distict { get; set; }
      public IDictionary<string, object> Arguments { get; private set; }
   }
}