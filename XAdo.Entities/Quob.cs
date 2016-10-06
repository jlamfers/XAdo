using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Sql;
using XAdo.Quobs.Sql.Formatter;

namespace XAdo.Quobs
{
   public class Quob : IQuob, ISqlBuilder
   {
      public Quob(string rawTableName, ISqlFormatter formatter, ISqlExecuter executer)
      {
         if (rawTableName == null) throw new ArgumentNullException("rawTableName");
         if (formatter == null) throw new ArgumentNullException("formatter");
         if (executer == null) throw new ArgumentNullException("executer");
         Formatter = formatter;
         Executer = executer;
         Meta = new SqlSelectMeta { TableName = rawTableName };
      }

      public virtual IQuob Select(params SelectColumn[] raw)
      {
         Meta.SelectColumns.AddRange(raw);
         return this;
      }
      public virtual IQuob Select(params string[] raw)
      {
         Meta.SelectColumns.AddRange(raw.Select(s => new SelectColumn(s)));
         return this;
      }

      public virtual IQuob Where(params string[] raw)
      {
         Meta.WhereClausePredicates.CastTo<List<string>>().AddRange(raw);
         return this;
      }

      public virtual IQuob Having(params string[] raw)
      {
         Meta.HavingClausePredicates.CastTo<List<string>>().AddRange(raw);
         return this;
      }

      public virtual IQuob OrderBy(params OrderColumn[] columns)
      {
         Meta.OrderColumns.AddRange(columns);
         return this;
      }

      public virtual IQuob GroupBy(params string[] raw)
      {
         Meta.GroupByColumns.CastTo<List<string>>().AddRange(raw);
         return this;
      }

      public IQuob Skip(int? value)
      {
         Meta.Arguments[SqlFormatter.ParNameSkip] = value;
         return this;
      }

      public IQuob Take(int? value)
      {
         Meta.Arguments[SqlFormatter.ParNameTake] = value;
         return this;
      }

      public virtual IQuob Distinct()
      {
         Meta.CastTo<SqlSelectMeta>().Distict = true;
         return this;
      }

      public virtual long Count()
      {
         using (var w = new StringWriter())
         {
            Formatter.FormatSqlSelectCount(w, Meta);
            var sql = w.GetStringBuilder().ToString();
            return Executer.ExecuteScalar<long>(sql, Meta.Arguments);
         }
      }
      public virtual IEnumerable<dynamic> ToEnumerable()
      {
         return Executer.ExecuteQuery<dynamic>(GetSql(), Meta.Arguments);
      }
      public virtual IList<dynamic> ToList()
      {
         var list = ToEnumerable();
         return (list as IList<dynamic>) ?? list.ToList();
      }
      public virtual IEnumerable<dynamic> ToEnumerable(out long count)
      {
         using (var w = new StringWriter())
         {
            Formatter.FormatSqlSelectCount(w, Meta);
            var sqlCount = w.GetStringBuilder().ToString();
            return Executer.ExecuteQuery<dynamic>(GetSql(), Meta.Arguments, sqlCount, out count);
         }

      }
      public virtual IList<dynamic> ToList(out long count)
      {
         var list = ToEnumerable(out count);
         return (list as IList<dynamic>) ?? list.ToList();
      }

      protected virtual ISqlFormatter Formatter { get; private set; }
      protected virtual ISqlSelectMeta Meta { get; private set; }
      protected virtual ISqlExecuter Executer { get; private set; }

      public virtual string GetSql()
      {
         using (var w = new StringWriter())
         {
            var parNameSkip = Meta.Arguments.ContainsKey(SqlFormatter.ParNameSkip) ? SqlFormatter.ParNameSkip : null;
            var parNameTake = Meta.Arguments.ContainsKey(SqlFormatter.ParNameTake) ? SqlFormatter.ParNameTake : null;
            if (parNameSkip != null || parNameTake != null)
            {
               Formatter.FormatSqlSelectPaged(w, Meta,parNameSkip, parNameTake);
            }
            else
            {
               Formatter.FormatSqlSelect(w, Meta);
            }
            return w.GetStringBuilder().ToString();
         }
      }
      public virtual IDictionary<string, object> GetArguments()
      {
         return Meta.Arguments.ToDictionary(x => x.Key, x => x.Value);
      }
   }
}