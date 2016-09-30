using System.Collections.Generic;

namespace XAdo.Entities.Sql.Formatter
{
   public interface ISqlSelectMeta
   {
      IList<string> SelectColumns { get; }
      IList<string> WhereClausePredicates { get; }
      IList<string> HavingClausePredicates { get; }
      IList<string> OrderColumns { get; }
      IList<string> GroupByColumns { get; }
      string TableName { get; }
      IDictionary<string, object> Arguments { get; }
   }
}