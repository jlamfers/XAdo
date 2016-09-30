using System.Collections.Generic;
using XAdo.Entities.Sql.Formatter;

namespace XAdo.Entities.Sql
{
   public class SqlSelectMeta : ISqlSelectMeta
   {
      public SqlSelectMeta()
      {
         GroupByColumns = new List<string>();
         OrderColumns = new List<string>();
         HavingClausePredicates = new List<string>();
         WhereClausePredicates = new List<string>();
         SelectColumns = new List<string>();
         Arguments = new Dictionary<string, object>();
      }

      public IList<string> SelectColumns { get; private set; }
      public IList<string> WhereClausePredicates { get; private set; }
      public IList<string> HavingClausePredicates { get; private set; }
      public IList<string> OrderColumns { get; private set; }
      public IList<string> GroupByColumns { get; private set; }
      public string TableName { get; set; }
      public IDictionary<string, object> Arguments { get; private set; }
   }
}