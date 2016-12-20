using System;
using System.Linq;
using System.Linq.Expressions;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlExpression.Visitors;
using XAdo.SqlObjects.SqlObjects.Core;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects
{
   public class QuerySqlObject<TTable> : FetchSqlObject<TTable>, IQuerySqlObject
      where TTable : IDbTable
   {
      public QuerySqlObject(ISqlFormatter formatter)
         : base(formatter, null, new QueryChunks(new Aliases()) { TableName = typeof(TTable).GetTableDescriptor().Format(formatter) }, null)
      {
         
      }
      public QuerySqlObject(ISqlConnection connection)
         : base(connection.GetSqlFormatter(), connection, new QueryChunks(new Aliases()) { TableName = typeof(TTable).GetTableDescriptor().Format(connection.GetSqlFormatter()) }, null)
      {
      }

      internal Action<Expression, SqlBuilderContext> CallbackWriter;

      protected override IReadSqlObject Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new SqlExpressionVisitor();
         var context = new JoinBuilderContext(Formatter, Chunks.Aliases, Joins){CallbackWriter = CallbackWriter};

         sqlBuilder.BuildSql(context, expression);

         Chunks.WhereClausePredicates.Add(context.ToString());
         Chunks.AddJoins(context.JoinChunks);

         foreach (var arg in context.Arguments)
         {
            Chunks.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      protected override IReadSqlObject OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         if (!keepOrder)
         {
            Chunks.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionVisitor();
            var context = new JoinBuilderContext(Formatter, Chunks.Aliases,Joins);

            sqlBuilder.BuildSql(context, expression);
            Chunks.AddJoins(context.JoinChunks);
            Chunks.OrderColumns.Add(new QueryChunks.OrderColumn(context.ToString(), descending));
            return this;
         }
         return this;
      }

      public virtual MappedSqlObject<TMapped> Map<TMapped>(Expression<Func<TTable, TMapped>> mapExpression)
      {
         var chunks = Chunks.Clone();
         var result = PrepareMapExpression<TMapped>(mapExpression, chunks);
         return new MappedSqlObject<TMapped>(Formatter, Connection, result.BinderExpression.Compile(), chunks, result, Joins);
      }

      private BinderExpressionVisitor.CompileResult<TMapped> PrepareMapExpression<TMapped>(LambdaExpression mapExpression, QueryChunks chunks)
      {
         var compiler = new BinderExpressionVisitor(Formatter);
         var result = compiler.Compile<TMapped>(mapExpression, Joins);
         chunks.AddJoins(result.Joins);
         chunks.SelectColumns.AddRange(result.Columns.Select(c => new QueryChunks.SelectColumn(c.Sql, c.Alias)).Distinct());
         foreach (var c in chunks.OrderColumns.Where(c => !chunks.SelectColumns.Any(sc => sc.Expression == c.Expression)))
         {
            chunks.SelectColumns.Add(new QueryChunks.SelectColumn(c.Expression, null));
         }
         chunks.EnsureSelectColumnsAreAliased();
         return result;
      }


      public virtual new QuerySqlObject<TTable> Distinct()
      {
         return (QuerySqlObject<TTable>)base.Distinct();
      }

      public virtual new QuerySqlObject<TTable> Skip(int skip)
      {
         return (QuerySqlObject<TTable>)base.Skip(skip);
      }

      public virtual new QuerySqlObject<TTable> Take(int take)
      {
         return (QuerySqlObject<TTable>)base.Take(take);
      }

      public virtual QuerySqlObject<TTable> OrderBy(params Expression<Func<TTable, object>>[] expressions)
      {
         return (QuerySqlObject<TTable>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual QuerySqlObject<TTable> OrderByDescending(params Expression<Func<TTable, object>>[] expressions)
      {
         return (QuerySqlObject<TTable>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual QuerySqlObject<TTable> AddOrderBy(params Expression<Func<TTable, object>>[] expressions)
      {
         return (QuerySqlObject<TTable>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual QuerySqlObject<TTable> AddOrderByDescending(params Expression<Func<TTable, object>>[] expressions)
      {
         return (QuerySqlObject<TTable>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public new virtual QuerySqlObject<TTable> Union(IReadSqlObject sqlReadObject)
      {
         return (QuerySqlObject<TTable>)base.Union(sqlReadObject);
      }

      public virtual QuerySqlObject<TTable> GroupBy(params Expression<Func<TTable, object>>[] expressions)
      {
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionVisitor();
            var context = new JoinBuilderContext(Formatter, Chunks.Aliases, Joins){CallbackWriter = CallbackWriter};

            sqlBuilder.BuildSql(context, expression);
            Chunks.AddJoins(context.JoinChunks);
            Chunks.GroupByColumns.Add(context.ToString());
            return this;
         }
         return this;
      }

      public virtual QuerySqlObject<TTable> Clone()
      {
         return (QuerySqlObject<TTable>)CloneSqlReadObject();
      }

      public virtual QuerySqlObject<TTable> Where(Expression<Func<TTable, bool>> whereClause)
      {
         if (whereClause == null) return this;
         this.CastTo<IReadSqlObject>().Where(whereClause);
         return this;
      }

      public virtual QuerySqlObject<TTable> Having(Expression<Func<TTable, bool>> havingClause)
      {
         var sqlBuilder = new SqlExpressionVisitor();
         var context = new JoinBuilderContext(Formatter, Chunks.Aliases,Joins) { CallbackWriter = CallbackWriter};

         sqlBuilder.BuildSql(context, havingClause);
         Chunks.AddJoins(context.JoinChunks);
         Chunks.HavingClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Chunks.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }


      protected override ReadSqlObject CloneSqlReadObject()
      {
         //we need to clone joins as well here
         return new QuerySqlObject<TTable>(Connection) { Chunks = Chunks.Clone(), Joins = Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList()};
      }



   }
}