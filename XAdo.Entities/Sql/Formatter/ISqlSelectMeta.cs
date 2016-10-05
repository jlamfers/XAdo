using System;
using System.Collections.Generic;

namespace XAdo.Quobs.Sql.Formatter
{
   public interface ISqlSelectMeta
   {
      // <column statement>,<alias> => alias is optional
      List<Tuple<string,string>> SelectColumns { get; }
      List<string> WhereClausePredicates { get; }
      List<string> HavingClausePredicates { get; }
      List<string> OrderColumns { get; }
      List<string> GroupByColumns { get; }
      string TableName { get; }
      bool Distict { get; }
      IDictionary<string, object> Arguments { get; }
   }

}