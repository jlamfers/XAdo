using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Attributes;

namespace XAdo.Quobs.Meta
{
   //NOTE: at this level all sql is (must be) formatted, and provider specific
   public static class SqlDescriptor
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

         public SelectColumnDescriptor(string expression, string alias = null)
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
            return new SelectColumnDescriptor(Expression,Alias);
         }
      }
      public class OrderColumnDescriptor
      {
         public OrderColumnDescriptor() { }

         public OrderColumnDescriptor(string expression, bool descending = false)
         {
            Expression = expression;
            Descending = descending;
         }

         public string Expression { get; set; }
         public bool Descending { get; set; }

         public override string ToString()
         {
            return string.Format("{0}{1}", Expression, Descending ? " DESC" : "");
         }

         public OrderColumnDescriptor Clone()
         {
            return new OrderColumnDescriptor(Expression,Descending);
         }
      }
      public class JoinDescriptor : SchemaDescriptor.JoinDescriptor
      {
         private readonly int _hashcode;

         public JoinDescriptor(string expression, JoinType joinType) : base(expression)
         {
            JoinType = joinType;
            _hashcode = expression.GetHashCode();
         }


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

         public JoinDescriptor Clone()
         {
            return new JoinDescriptor(Expression,JoinType);
         }
      }

      public class SelectDescriptor
      {
         public SelectDescriptor()
         {
            SelectColumns = new List<SelectColumnDescriptor>();
            DiscriminatorPredicates = new List<string>();
            WhereClausePredicates = new List<string>();
            HavingClausePredicates = new List<string>();
            OrderColumns = new List<OrderColumnDescriptor>();
            GroupByColumns = new List<string>();
            Joins = new List<JoinDescriptor>();
            WhereClauseJoins = new List<JoinDescriptor>();
            Arguments = new Dictionary<string, object>();
         }

         public List<SelectColumnDescriptor> SelectColumns { get; private set; }
         public List<string> DiscriminatorPredicates { get; private set; }
         public List<string> WhereClausePredicates { get; private set; }
         public List<string> HavingClausePredicates { get; private set; }
         public List<OrderColumnDescriptor> OrderColumns { get; private set; }
         public List<string> GroupByColumns { get; private set; }
         public List<JoinDescriptor> Joins { get; private set; }
         public List<JoinDescriptor> WhereClauseJoins { get; private set; }
         public string TableName { get; set; }
         public bool Distict { get; set; }
         public IDictionary<string, object> Arguments { get; private set; }

         public void WriteSelect(TextWriter writer)
         {
            var self = this;

            var distinct = self.Distict ? "DISTINCT " : "";

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
            if (self.Joins.Any() || self.WhereClauseJoins.Any())
            {
               writer.WriteLine("   " +
                           String.Join("\r\n   ",
                              self.Joins.Concat(self.WhereClauseJoins).Select(j => j.ToString()).ToArray()));
            }
            if (self.WhereClausePredicates.Any() || self.DiscriminatorPredicates.Any())
            {
               writer.WriteLine("WHERE");
               writer.WriteLine("   " +
                           String.Join("\r\n   AND ",
                              self.DiscriminatorPredicates.Concat(self.WhereClausePredicates)
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
            if (self.OrderColumns.Any())
            {
               writer.WriteLine("ORDER BY");
               writer.WriteLine("   " + String.Join(",\r\n   ", self.OrderColumns.Select(c => c.ToString()).ToArray()));
            }
         }
         public void WriteCount(TextWriter writer)
         {
            var clone = this;
            if (clone.OrderColumns.Any())
            {
               clone = Clone();
               clone.OrderColumns.Clear();
            }
            writer.Write("SELECT COUNT(1) FROM (");
            clone.WriteSelect(writer);
            writer.Write(") AS t");
         }

         public override string ToString()
         {
            using (var w = new StringWriter())
            {
               WriteSelect(w);
               return w.GetStringBuilder().ToString();
            }
         }

         public SelectDescriptor Clone()
         {
            var clone = new SelectDescriptor();
            clone.SelectColumns.AddRange(SelectColumns.Select(c => c.Clone()));
            clone.DiscriminatorPredicates.AddRange(DiscriminatorPredicates);
            clone.WhereClausePredicates.AddRange(WhereClausePredicates);
            clone.HavingClausePredicates.AddRange(HavingClausePredicates);
            clone.OrderColumns.AddRange(OrderColumns.Select(c => c.Clone()));
            clone.GroupByColumns.AddRange(GroupByColumns);
            clone.Joins.AddRange(Joins.Select(c => c.Clone()));
            clone.WhereClauseJoins.AddRange(WhereClauseJoins.Select(c => c.Clone()));
            clone.TableName = TableName;
            clone.Distict = Distict;
            clone.Arguments = Arguments.ToDictionary(e => e.Key, e => e.Value);
            return clone;
         }
      }
   }
}
