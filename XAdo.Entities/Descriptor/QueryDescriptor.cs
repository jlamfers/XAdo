﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Attributes;

namespace XAdo.Quobs.Descriptor
{
   //NOTE: at this level all sql is (must be) formatted, and provider specific
   public class QueryDescriptor
   {
      public static class Constants
      {
         public const string
            ParNameSkip = "___s_kip",
            ParNameTake = "___t_ake";
      }


      public class SelectColumnDescriptor
      {
         public SelectColumnDescriptor() { }

         public SelectColumnDescriptor(string expression, string alias)
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

         public SelectColumnDescriptor Clone()
         {
            return new SelectColumnDescriptor(Expression, Alias);
         }
      }

      public class OrderColumnDescriptor
      {
         public OrderColumnDescriptor() { }

         public OrderColumnDescriptor(string expression, bool descending = false)
         {
            this.Expression = expression;
            Descending = @descending;
         }

         public string Expression { get; set; }
         public string Alias { get; set; }
         public bool Descending { get; set; }


         public override string ToString()
         {
            return String.Format("{0}{1}", this.Alias, Descending ? " DESC" : "");
         }

         public OrderColumnDescriptor Clone()
         {
            return new OrderColumnDescriptor(this.Expression, Descending){Alias = Alias};
         }
      }

      public class JoinDescriptor 
      {
         private readonly int _hashcode;

         public JoinDescriptor(string expression, JoinType joinType)
         {
            Expression = expression;
            JoinType = joinType;
            _hashcode = expression.GetHashCode();
         }

         public JoinDescriptor(SchemaDescriptor.JoinDescriptor other)
            : this(other.Expression, other.JoinType)
         {
         }

         public string Expression { get; private set; }
         public JoinType JoinType { get; set; }


         // note: equality does not depend on join type
         public override int GetHashCode()
         {
            return _hashcode;
         }

         public override bool Equals(object obj)
         {
            var other = obj as JoinDescriptor;
            return other != null && other.Expression == Expression;
         }

         public override string ToString()
         {
            return JoinType.ToSqlJoinExpression(Expression);
         }

         public JoinDescriptor Clone()
         {
            return new JoinDescriptor(Expression, JoinType);
         }
      }

      public QueryDescriptor()
      {
         SelectColumns = new List<SelectColumnDescriptor>();

         DiscriminatorClausePredicates = new List<string>();
         DiscriminatorArguments = new Dictionary<string, object>();

         WhereClausePredicates = new List<string>();
         Arguments = new Dictionary<string, object>();

         GroupByColumns = new List<string>();

         HavingClausePredicates = new List<string>();
         OrderColumns = new List<OrderColumnDescriptor>();
         Joins = new List<JoinDescriptor>();
      }

      public List<SelectColumnDescriptor> SelectColumns { get; private set; }
      public List<string> DiscriminatorClausePredicates { get; private set; }
      public List<string> WhereClausePredicates { get; private set; }
      public List<string> HavingClausePredicates { get; private set; }
      public List<OrderColumnDescriptor> OrderColumns { get; private set; }
      public List<string> GroupByColumns { get; private set; }
      public List<JoinDescriptor> Joins { get; private set; }
      public string TableName { get; set; }
      public bool Distict { get; set; }
      public IDictionary<string, object> Arguments { get; private set; }
      public IDictionary<string, object> DiscriminatorArguments { get; private set; }

      public int? Skip
      {
         get
         {
            object skip;
            return Arguments.TryGetValue(Constants.ParNameSkip, out skip) ? (int?) skip : null;
         }
         set { Arguments[Constants.ParNameSkip] = value; }
      }
      public int? Take
      {
         get
         {
            object take;
            return Arguments.TryGetValue(Constants.ParNameTake, out take) ? (int?)take : null;
         }
         set { Arguments[Constants.ParNameTake] = value; }
      }
      public bool IsPaged()
      {
         return Skip != null || Take != null;
      }

      public void WriteSelect(TextWriter writer, bool ignoreOrder = false)
      {
         var self = this;

         var distinct = self.Distict ? "DISTINCT " : "";

         EnsureOrderByColumnsAreAliased();

         if (!self.SelectColumns.Any())
         {
            writer.WriteLine("SELECT {0}*", distinct);
         }
         else
         {
            writer.WriteLine("SELECT {0}", distinct);
            writer.WriteLine("   " +
                        String.Join(",\r\n   ",
                           self.SelectColumns.Select(
                              t => String.IsNullOrEmpty(t.Alias) ? t.Expression : t.Expression + " AS " + t.Alias)));
         }
         writer.WriteLine("FROM {0}", self.TableName);
         if (self.Joins.Any())
         {
            writer.WriteLine("   " +
                        String.Join("\r\n   ",
                           self.Joins.Select(j => j.ToString()).ToArray()));
         }
         if (self.WhereClausePredicates.Any() || self.DiscriminatorClausePredicates.Any())
         {
            writer.WriteLine("WHERE");
            writer.WriteLine("   " +
                        String.Join("\r\n   AND ",
                           self.DiscriminatorClausePredicates.Concat(self.WhereClausePredicates)
                              .Select(s => "(" + s + ")")
                              .ToArray()));
         }
         if (self.GroupByColumns.Any())
         {
            writer.WriteLine("GROUP BY");
            writer.WriteLine("   " + String.Join(",\r\n   ", self.GroupByColumns.ToArray()));
         }
         if (self.HavingClausePredicates.Any())
         {
            writer.WriteLine("HAVING");
            writer.WriteLine("   " +
                        String.Join("\r\n   AND ",
                           self.HavingClausePredicates.Select(s => "(" + s + ")").ToArray()));
         }
         if (!ignoreOrder && self.OrderColumns.Any())
         {
            writer.WriteLine("ORDER BY");
            writer.WriteLine("   " + String.Join(",\r\n   ", self.OrderColumns.Select(c => c.ToString()).ToArray()));
         }
      }
      public void WriteCount(TextWriter writer)
      {
         writer.Write("SELECT COUNT(1) FROM (");
         WriteSelect(writer,true);
         writer.Write(") AS __t");
      }

      public override string ToString()
      {
         using (var w = new StringWriter())
         {
            WriteSelect(w);
            return w.GetStringBuilder().ToString();
         }
      }

      public QueryDescriptor Clone(bool reset = false)
      {
         var clone = new QueryDescriptor();
         clone.SelectColumns.AddRange(SelectColumns.Select(c => c.Clone()));
         clone.DiscriminatorClausePredicates.AddRange(DiscriminatorClausePredicates);
         clone.HavingClausePredicates.AddRange(HavingClausePredicates);
         clone.OrderColumns.AddRange(OrderColumns.Select(c => c.Clone()));
         clone.GroupByColumns.AddRange(GroupByColumns);
         clone.Joins.AddRange(Joins.Select(c => c.Clone()));
         clone.TableName = TableName;
         clone.Distict = Distict;
         clone.DiscriminatorArguments = DiscriminatorArguments.ToDictionary(e => e.Key, e => e.Value);
         if (!reset)
         {
            clone.Arguments = Arguments.ToDictionary(e => e.Key, e => e.Value);
            clone.WhereClausePredicates.AddRange(WhereClausePredicates);
         }
         return clone;
      }

      public IDictionary<string, object> GetArguments()
      {
         var dict = Arguments.ToDictionary(i => i.Key, i => i.Value);
         foreach (var item in DiscriminatorArguments)
         {
            dict.Add(item.Key, item.Value);
         }
         return dict;
      }

      public QueryDescriptor AddJoins(IEnumerable<JoinDescriptor> joins)
      {
         foreach (var j in joins.Where(j => !Joins.Contains(j)))
         {
            Joins.Add(j);
         }
         return this;
      }

      public void EnsureSelectColumnsAreAliased()
      {
         var i = 0;
         foreach (var c in SelectColumns)
         {
            if (c.Alias == null)
            {
               c.Alias = "_c_" + (i++);
            }
         }
      }

      public void EnsureOrderByColumnsAreAliased()
      {
         foreach (var c in OrderColumns)
         {
            if (c.Alias == null)
            {
               c.Alias = SelectColumns.Where(s => s.Expression == c.Expression).Select(s => s.Alias).SingleOrDefault();
            }
         }
      }
   }
}