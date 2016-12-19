using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlExpression.Visitors;
using XAdo.SqlObjects.SqlObjects.Core;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects
{
   public class TableSqlObject<TTable> : FetchSqlObject<TTable>, ITableSqlObject<TTable> 
      where TTable : IDbTable
   {
      public TableSqlObject(ISqlFormatter formatter)
         : base(formatter, null, new QueryChunks(new Aliases()) { TableName = typeof(TTable).GetTableDescriptor().Format(formatter) }, null)
      {
         
      }
      public TableSqlObject(ISqlConnection connection)
         : base(connection.GetSqlFormatter(), connection, new QueryChunks(new Aliases()) { TableName = typeof(TTable).GetTableDescriptor().Format(connection.GetSqlFormatter()) }, null)
      {
      }

      internal Action<Expression, SqlBuilderContext> ParentMemberWriter;

      protected override IReadSqlObject Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new SqlExpressionVisitor();
         var context = new JoinBuilderContext(Formatter, Chunks.Aliases, Joins){ParentWriter = ParentMemberWriter};

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

      public virtual IMappedSqlObject<TMapped> Map<TMapped>(Expression<Func<TTable, TMapped>> mapExpression)
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


      public virtual new ITableSqlObject<TTable> Distinct()
      {
         return (ITableSqlObject<TTable>)base.Distinct();
      }

      public virtual new ITableSqlObject<TTable> Skip(int skip)
      {
         return (ITableSqlObject<TTable>)base.Skip(skip);
      }

      public virtual new ITableSqlObject<TTable> Take(int take)
      {
         return (ITableSqlObject<TTable>)base.Take(take);
      }

      public virtual ITableSqlObject<TTable> OrderBy(params Expression<Func<TTable, object>>[] expressions)
      {
         return (ITableSqlObject<TTable>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ITableSqlObject<TTable> OrderByDescending(params Expression<Func<TTable, object>>[] expressions)
      {
         return (ITableSqlObject<TTable>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual ITableSqlObject<TTable> AddOrderBy(params Expression<Func<TTable, object>>[] expressions)
      {
         return (ITableSqlObject<TTable>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ITableSqlObject<TTable> AddOrderByDescending(params Expression<Func<TTable, object>>[] expressions)
      {
         return (ITableSqlObject<TTable>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public new virtual ITableSqlObject<TTable> Union(IReadSqlObject sqlReadObject)
      {
         return (ITableSqlObject<TTable>)base.Union(sqlReadObject);
      }

      public virtual ITableSqlObject<TTable> GroupBy(params Expression<Func<TTable, object>>[] expressions)
      {
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionVisitor();
            var context = new JoinBuilderContext(Formatter, Chunks.Aliases, Joins){ParentWriter = ParentMemberWriter};

            sqlBuilder.BuildSql(context, expression);
            Chunks.AddJoins(context.JoinChunks);
            Chunks.GroupByColumns.Add(context.ToString());
            return this;
         }
         return this;
      }

      public virtual ITableSqlObject<TTable> Clone()
      {
         return (ITableSqlObject<TTable>)CloneSqlReadObject();
      }

      public virtual ITableSqlObject<TTable> Where(Expression<Func<TTable, bool>> whereClause)
      {
         if (whereClause == null) return this;
         this.CastTo<IReadSqlObject>().Where(whereClause);
         return this;
      }

      public virtual ITableSqlObject<TTable> Having(Expression<Func<TTable, bool>> havingClause)
      {
         var sqlBuilder = new SqlExpressionVisitor();
         var context = new JoinBuilderContext(Formatter, Chunks.Aliases,Joins) { ArgumentsAsLiterals = false, ParentWriter = ParentMemberWriter};

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
         return new TableSqlObject<TTable>(Connection) { Chunks = Chunks.Clone(), Joins = Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList()};
      }



   }
}