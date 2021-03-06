﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.Dialects.SqlServer;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects.Core
{
   //NOTE: at this level all sql is (must be) formatted, and provider specific
   public class QueryChunks
   {
      public static class Constants
      {
         public const string
            ParNameSkip = "__skip_",
            ParNameTake = "__take_";
      }

      public class SelectColumn
      {
         public SelectColumn(string expression, string alias)
         {
            Expression = expression;
            Alias = alias;
         }
         public string Expression { get; set; }
         public string Alias { get; set; }

         public override string ToString()
         {
            return Expression + (Alias != null ? (" AS " + Alias) : "");
         }

         public SelectColumn Clone()
         {
            return new SelectColumn(Expression, Alias);
         }

         public override int GetHashCode()
         {
            unchecked
            {
               return Expression.GetHashCode()*127 + (Alias ?? "").GetHashCode();
            }
         }

         public override bool Equals(object obj)
         {
            var other = obj as SelectColumn;
            return other != null && other.Expression == Expression && other.Alias == Alias;
         }
      }

      public class OrderColumn
      {
         public OrderColumn(string expression, bool descending = false)
         {
            Expression = expression;
            Descending = @descending;
         }

         public OrderColumn(string expression, string alias, bool descending = false)
         {
            Expression = expression;
            Alias = alias;
            Descending = @descending;
         }

         public string Expression { get; set; }
         public string Alias { get; set; }
         public bool Descending { get; set; }


         public override string ToString()
         {
            return String.Format("{0}{1}", this.Alias, Descending ? " DESC" : "");
         }

         public OrderColumn Clone()
         {
            return new OrderColumn(this.Expression, Descending){Alias = Alias};
         }
      }

      public class Join 
      {
         private readonly int _hashcode;

         public Join(string expression, string joinType)
         {
            Expression = expression;
            JoinType = joinType;
            _hashcode = expression.GetHashCode();
         }

         public string Expression { get; private set; }
         public string JoinType { get; set; }


         // note: equality does not depend on join type
         public override int GetHashCode()
         {
            return _hashcode;
         }

         public override bool Equals(object obj)
         {
            var other = obj as Join;
            return other != null && other.Expression == Expression;
         }

         public override string ToString()
         {
            return JoinType + " " + Expression;
         }

         public Join Clone()
         {
            return new Join(Expression, JoinType);
         }
      }

      public QueryChunks(IAliases aliases)
      {
         if (aliases == null) throw new ArgumentNullException("aliases");

         SelectColumns = new List<SelectColumn>();

         WhereClausePredicates = new List<string>();
         Arguments = new Dictionary<string, object>();

         GroupByColumns = new List<string>();

         HavingClausePredicates = new List<string>();
         Unions = new List<ISqlObject>();
         OrderColumns = new List<OrderColumn>();
         Joins = new List<Join>();
         Aliases = aliases;
      }

      public List<SelectColumn> SelectColumns { get; private set; }
      public List<string> WhereClausePredicates { get; private set; }
      public List<string> HavingClausePredicates { get; private set; }
      public List<ISqlObject> Unions { get; private set; }
      public List<OrderColumn> OrderColumns { get; private set; }
      public List<string> GroupByColumns { get; private set; }
      public List<Join> Joins { get; private set; }
      public string TableName { get; set; }
      public IAliases Aliases { get; set; }
      public bool Distict { get; set; }
      public IDictionary<string, object> Arguments { get; private set; }

      public virtual int? Skip
      {
         get
         {
            object skip;
            return Arguments.TryGetValue(Constants.ParNameSkip, out skip) ? (int?) skip : null;
         }
         set { Arguments[Constants.ParNameSkip] = value; }
      }
      public virtual int? Take
      {
         get
         {
            object take;
            return Arguments.TryGetValue(Constants.ParNameTake, out take) ? (int?)take : null;
         }
         set { Arguments[Constants.ParNameTake] = value; }
      }
      public virtual bool IsPaged()
      {
         return Skip != null || Take != null;
      }

      public override string ToString()
      {
         using (var w = new StringWriter())
         {
            new SqlFormatter(new SqlServerDialect()).WriteSelect(w,this);
            return w.GetStringBuilder().ToString();
         }
      }

      public virtual QueryChunks Clone(bool reset = false)
      {
         var clone = new QueryChunks(Aliases);
         clone.SelectColumns.AddRange(SelectColumns.Select(c => c.Clone()));
         clone.HavingClausePredicates.AddRange(HavingClausePredicates);
         clone.OrderColumns.AddRange(OrderColumns.Select(c => c.Clone()));
         clone.GroupByColumns.AddRange(GroupByColumns);
         clone.Joins.AddRange(Joins.Select(c => c.Clone()));
         clone.TableName = TableName;
         clone.Distict = Distict;
         clone.Unions = Unions.ToList();
         if (!reset)
         {
            clone.Arguments = Arguments.ToDictionary(e => e.Key, e => e.Value);
            clone.WhereClausePredicates.AddRange(WhereClausePredicates);
         }
         return clone;
      }

      public virtual IDictionary<string, object> GetArguments()
      {
         var dict = Arguments.ToDictionary(i => i.Key, i => i.Value);
         foreach (var union in Unions)
         {
            foreach (var kv in EnsureDictionary(union.GetArguments()))
            {
               dict[kv.Key] = kv.Value;
            }
         }
         return dict;
      }

      private IDictionary<string, object> EnsureDictionary(object arguments)
      {
         if (arguments == null) return null;
         var dict = arguments as IDictionary<string, object>;
         if (dict != null) return dict;
         return arguments.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(arguments));
      }

      public virtual void AddJoins(IEnumerable<Join> joins)
      {
         foreach (var j in joins.Where(j => !Joins.Contains(j)))
         {
            Joins.Add(j);
         }
      }

      public virtual void EnsureSelectColumnsAreAliased()
      {
         var i = 0;
         foreach (var c in SelectColumns)
         {
            if (c.Alias == null)
            {
               c.Alias = Aliases.Column(i);
            }
            i++;
         }
      }

      public virtual void EnsureOrderByColumnsAreAliased()
      {
         foreach (var c in OrderColumns)
         {
            if (c.Alias == null)
            {
               c.Alias = SelectColumns.Where(s => s.Expression == c.Expression).Select(s => s.Alias).SingleOrDefault() ?? c.Expression;
            }
         }
      }
   }
}
