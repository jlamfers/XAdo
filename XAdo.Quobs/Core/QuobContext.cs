using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core
{
   public class QuobContext : SqlBuilderContext
   {
      private Dictionary<string, DbSchemaDescriptor.JoinDescriptor>
         _schemaJoins;

      public QuobContext(ISqlFormatter formatter,Dictionary<string, DbSchemaDescriptor.JoinDescriptor> schemaJoins = null)
         : base(formatter)
      {
         VisitorHook = _VisitorHook;
         _schemaJoins = schemaJoins ?? new Dictionary<string, DbSchemaDescriptor.JoinDescriptor>();
      }

      public IEnumerable<QueryDescriptor.JoinDescriptor> GetJoins(Type startTable)
      {
         return _schemaJoins.Values.Sort(startTable).Select(j => new QueryDescriptor.JoinDescriptor(Formatter.FormatJoin(j.Expression), j.JoinType.ToJoinTypeString()));
      }

      private Expression _VisitorHook(ExpressionVisitor visitor, SqlBuilderContext context, Expression exp)
      {
         var m = exp as MethodCallExpression;
         if (m == null) return null;
         var joins = m.Method.GetJoinDescriptors().ToArray();
         if (joins.Length == 0) return null;
         var joinType = m.Arguments.Count > 1 ? (JoinType)m.Arguments[1].GetExpressionValue() : JoinType.Inner;
         foreach (var join in joins)
         {
            if (!_schemaJoins.ContainsKey(join.Expression))
            {
               join.JoinType = joinType;
               _schemaJoins.Add(join.Expression, join);
            }
         }
         visitor.Visit(m.Arguments[0]);
         return exp;
      }

   }
}