using System;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core.SqlExpression
{
   public abstract class SqlAggregateAttribute : CustomSqlExpressionBuilderAttribute
   {
   }

   public static partial class SqlAggregate
   {
      private class SqlAggregateBuilder : ICustomSqlExpressionBuilder
      {
         private readonly string _aggregateName;

         public SqlAggregateBuilder(string aggregateName)
         {
            _aggregateName = aggregateName;
         }

         public void BuildSql(ExpressionVisitor parent, SqlBuilderContext context, Expression expression)
         {
            var e = expression as MethodCallExpression;
            if (e == null)
            {
               throw new ArgumentException("Expected a MethodCallExpression", "expression");
            }
            var w = context.Writer;
            w.Write(_aggregateName);
            w.Write("(");
            var type = e.Method.GetGenericArguments().FirstOrDefault();
            if (type != null && type != e.Arguments[0].Type)
            {
               var f = context.Formatter;
               f.WriteTypeCast(w, type, w2 =>
               {
                  context.Writer = w2 ?? context.Writer;
                  parent.Visit(e.Arguments[0]);
                  context.Writer = w;
               });
            }
            else
            {
               parent.Visit(e.Arguments[0]);
            }
            w.Write(")");
         }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public class AvgAttribute : SqlAggregateAttribute
      {
         public AvgAttribute()
         {
            Builder = new SqlAggregateBuilder("AVG");
         }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public class MinAttribute : SqlAggregateAttribute
      {
         public MinAttribute()
         {
            Builder = new SqlAggregateBuilder("MIN");
         }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public class MaxAttribute : SqlAggregateAttribute
      {
         public MaxAttribute()
         {
            Builder = new SqlAggregateBuilder("MAX");
         }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public class CountAttribute : SqlAggregateAttribute
      {
         public CountAttribute()
         {
            Builder = new SqlAggregateBuilder("COUNT");
         }
      }
   }
}