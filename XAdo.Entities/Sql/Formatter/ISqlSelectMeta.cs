using System;
using System.Collections.Generic;

namespace XAdo.Quobs.Sql.Formatter
{
   public interface ISqlSelectMeta
   {
      // <column statement>,<alias> => alias is optional
      List<SelectColumn> SelectColumns { get; }
      List<string> WhereClausePredicates { get; }
      List<string> HavingClausePredicates { get; }
      List<OrderColumn> OrderColumns { get; }
      List<string> GroupByColumns { get; }
      string TableName { get; }
      bool Distict { get; }
      IDictionary<string, object> Arguments { get; }
   }

}