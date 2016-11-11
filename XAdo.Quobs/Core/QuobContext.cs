using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core
{
   public class QuobContext : SqlBuilderContext
   {
      private readonly List<DbSchemaDescriptor.JoinPath>
         _joins;

      public QuobContext(ISqlFormatter formatter, List<DbSchemaDescriptor.JoinPath> joins = null)
         : base(formatter)
      {
         VisitorHook = _VisitorHook;
         _joins = joins ?? new List<DbSchemaDescriptor.JoinPath>();
      }

      public IEnumerable<DbSchemaDescriptor.JoinPath> Joins
      {
         get { return _joins.Distinct(); }
      }

      [Obsolete]
      public IEnumerable<QueryDescriptor.JoinDescriptor> QuobJoins
      {
         get { return Joins.SelectMany(j => j.Joins).Select(j =>  new QueryDescriptor.JoinDescriptor(j.JoinInfo.Format(Formatter.IdentifierDelimiterLeft,Formatter.IdentifierDelimiterRight),j.JoinType.ToJoinTypeString())); }
      }

      private Expression _VisitorHook(ExpressionVisitor visitor, SqlBuilderContext context, Expression exp)
      {
         var joinPath = exp.GetJoinPath();
         if (joinPath == null) return null;
         _joins.Add(joinPath);
         return exp;
      }
   }
}