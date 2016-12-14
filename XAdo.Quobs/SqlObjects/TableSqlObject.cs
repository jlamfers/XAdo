using System;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.SqlObjects.Core;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class TableSqlObject<TTable> : FetchSqlObject<TTable>, ITableSqlObject<TTable> 
      where TTable : IDbTable
   {

      public TableSqlObject(ISqlConnection connection)
         : base(connection.GetSqlFormatter(), connection, new QueryChunks { FromTableName = typeof(TTable).GetTableDescriptor().Format(connection.GetSqlFormatter()) }, null)
      {
      }

      protected override IReadSqlObject Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new JoinBuilderContext(Formatter, Joins);

         sqlBuilder.BuildSql(context, expression);
         Chunks.WhereClausePredicates.Add(context.ToString());
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
            var sqlBuilder = new SqlExpressionBuilder();
            var context = new JoinBuilderContext(Formatter, Joins);

            sqlBuilder.BuildSql(context, expression);
            Chunks.AddJoins(context.JoinChunks);
            Chunks.OrderColumns.Add(new QueryChunks.OrderColumn(context.ToString(), descending));
            return this;
         }
         return this;
      }

      public virtual IMappedSqlObject<TMapped> Map<TMapped>(Expression<Func<TTable, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression<TMapped>(mapExpression);
         return new MappedSqlObject<TMapped>(Formatter, Connection, result.BinderExpression.Compile(), Chunks, result, Joins);
      }

      private BinderExpressionCompiler.CompileResult<TMapped> PrepareMapExpression<TMapped>(LambdaExpression mapExpression)
      {
         var compiler = new BinderExpressionCompiler(Formatter);
         var result = compiler.Compile<TMapped>(mapExpression, Joins);
         Chunks.AddJoins(result.Joins);
         Chunks.SelectColumns.AddRange(result.Columns.Select(c => new QueryChunks.SelectColumn(c.Sql, c.Alias)).Distinct());
         Chunks.EnsureSelectColumnsAreAliased();
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
            var sqlBuilder = new SqlExpressionBuilder();
            var context = new JoinBuilderContext(Formatter, Joins);

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
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new JoinBuilderContext(Formatter, Joins) { ArgumentsAsLiterals = false };

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
         return new TableSqlObject<TTable>(Connection) { Chunks = Chunks.Clone(), Joins = Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList() };
      }



   }
}