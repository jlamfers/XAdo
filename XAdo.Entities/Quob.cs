using System.Collections.Generic;
using XAdo.Entities.Sql;
using XAdo.Entities.Sql.Formatter;

namespace XAdo.Entities
{
   public class Quob
   {
      public const string
         ParNameSkip = "__skip",
         ParNameTake = "__take";

      public Quob(string tableName, ISqlFormatter formatter)
      {
         Formatter = formatter ?? new SqlFormatter();
         Meta = new SqlSelectMeta { TableName = Formatter.DelimitIdentifier(tableName) };
      }

      public Quob Select(params string[] columns)
      {
         Meta.SelectColumns.CastTo<List<string>>().AddRange(columns);
         return this;
      }

      public Quob Where(params string[] predicates)
      {
         Meta.WhereClausePredicates.CastTo<List<string>>().AddRange(predicates);
         return this;
      }

      public Quob Having(params string[] predicates)
      {
         Meta.HavingClausePredicates.CastTo<List<string>>().AddRange(predicates);
         return this;
      }

      public Quob OrderBy(params string[] columns)
      {
         Meta.OrderColumns.CastTo<List<string>>().AddRange(columns);
         return this;
      }

      public Quob GroupBy(params string[] columns)
      {
         Meta.GroupByColumns.CastTo<List<string>>().AddRange(columns);
         return this;
      }

      public Quob Skip(int? value)
      {
         return RegisterArgument(ParNameSkip, value);
      }

      public Quob Take(int? value)
      {
         return RegisterArgument(ParNameTake, value);
      }

      protected Quob RegisterArgument(string name, object value)
      {
         if (value == null)
         {
            Meta.Arguments.Remove(name);
         }
         else
         {
            Meta.Arguments[name] = value;
         }
         return this;
      }

      protected ISqlFormatter Formatter { get; private set; }
      protected ISqlSelectMeta Meta { get; private set; }

   }
}
