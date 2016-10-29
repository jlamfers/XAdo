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

         public JoinDescriptor(SchemaDescriptor.JoinDescriptor other)
            : base(other.Expression)
         {
            JoinType = other.JoinType;
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

      public class QueryDescriptor
      {
         public QueryDescriptor()
         {
            SelectColumns = new List<SelectColumnDescriptor>();

            DiscriminatorPredicates = new List<string>();
            DiscriminatorArguments = new Dictionary<string, object>();

            WhereClausePredicates = new List<string>();
            Arguments = new Dictionary<string, object>();

            GroupByColumns = new List<string>();

            HavingClausePredicates = new List<string>();
            OrderColumns = new List<OrderColumnDescriptor>();
            Joins = new List<JoinDescriptor>();
         }

         public List<SelectColumnDescriptor> SelectColumns { get; private set; }
         public List<string> DiscriminatorPredicates { get; private set; }
         public List<string> WhereClausePredicates { get; private set; }
         public List<string> HavingClausePredicates { get; private set; }
         public List<OrderColumnDescriptor> OrderColumns { get; private set; }
         public List<string> GroupByColumns { get; private set; }
         public List<JoinDescriptor> Joins { get; private set; }
         public string TableName { get; set; }
         public bool Distict { get; set; }
         public IDictionary<string, object> Arguments { get; private set; }
         public IDictionary<string, object> DiscriminatorArguments { get; private set; }

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
            if (self.Joins.Any())
            {
               writer.WriteLine("   " +
                           String.Join("\r\n   ",
                              self.Joins.Select(j => j.ToString()).ToArray()));
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
            clone.DiscriminatorPredicates.AddRange(DiscriminatorPredicates);
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
               dict.Add(item.Key,item.Value);
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
      }
   }
}
