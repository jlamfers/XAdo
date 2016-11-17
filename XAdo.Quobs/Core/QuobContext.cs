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

      private int _tableAliasIndex;


      public QuobContext(ISqlFormatter formatter, List<DbSchemaDescriptor.JoinPath> joins = null)
         : base(formatter)
      {
         _joins = joins ?? new List<DbSchemaDescriptor.JoinPath>();
         _tableAliasIndex = _joins.SelectMany(j => j.Joins).Count();
      }

      public IEnumerable<DbSchemaDescriptor.JoinPath> Joins
      {
         get { return _joins.Distinct(); }
      }


      [Obsolete]
      public IEnumerable<QueryDescriptor.JoinDescriptor> QuobJoins
      {
         get { return Joins.SelectMany(j => j.Joins).Select(j =>  new QueryDescriptor.JoinDescriptor(j.JoinInfo.Format(Formatter,j.LeftTableAlias,j.RightTableAlias),j.JoinType.ToJoinTypeString())); }
      }

      public override void WriteFormattedColumn(MemberExpression exp)
      {
         var joinPath = exp.Expression.GetJoinPath();
         if (joinPath != null)
         {
            var other = _joins.FirstOrDefault(j => j.EqualsOrStartsWith(joinPath));
            if (other != null)
            {
               joinPath = other.Joins.Count == joinPath.Joins.Count
                  ? other
                  : new DbSchemaDescriptor.JoinPath(other.Joins.Take(joinPath.Joins.Count));
            }
            else
            {
               other = _joins.Where(j => joinPath.EqualsOrStartsWith(j)).OrderByDescending(j => j.Joins.Count).FirstOrDefault();
               if (other != null)
               {
                  // add joins to existing join path
                  other.Joins = other.Joins.Concat(joinPath.Joins.Skip(other.Joins.Count)).ToList();
               }
               else
               {
                  // it is a new path
                  _joins.Add(joinPath);
               }
            }
            // now set aliases
            foreach (var path in _joins)
            {
               for (var i = 0; i < path.Joins.Count; i++)
               {
                  var join = path.Joins[i];
                  if (i == 0)
                  {
                     if (join.RightTableAlias == null)
                     {
                        join.RightTableAlias = Aliases.Table(++_tableAliasIndex);
                     }
                  }
                  else
                  {
                     if (join.RightTableAlias == null)
                     {
                        join.RightTableAlias = Aliases.Table(++_tableAliasIndex);
                     }
                     if (join.LeftTableAlias == null)
                     {
                        join.LeftTableAlias = path.Joins[i - 1].RightTableAlias;
                     }
                  }
               
            }
            }
         }
         var descriptor = exp.Member.GetColumnDescriptor();
         var alias = default(string);
         if (joinPath != null)
         {
            alias = joinPath.Joins.Last().RightTableAlias;
         }
         Writer.Write(descriptor.Format(Formatter.IdentifierDelimiterLeft, Formatter.IdentifierDelimiterRight,alias));
      }

   }
}