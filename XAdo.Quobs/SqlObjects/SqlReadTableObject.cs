using System;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class SqlReadTableObject<T> : SqlFetchObject<T>, ISqlReadTableObject<T> 
      where T : IDbTable
   {

      public SqlReadTableObject(ISqlConnection connection)
         : base(connection.GetSqlFormatter(), connection, new QueryDescriptor { FromTableName = typeof(T).GetTableDescriptor().Format(connection.GetSqlFormatter()) }, null)
      {
      }

      protected override ISqlReadObject Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new QuobContext(Formatter, Joins);

         sqlBuilder.BuildSql(context, expression);
         Descriptor.WhereClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      protected override ISqlReadObject OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         if (!keepOrder)
         {
            Descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionBuilder();
            var context = new QuobContext(Formatter, Joins);

            sqlBuilder.BuildSql(context, expression);
            Descriptor.AddJoins(context.QuobJoins);
            Descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(context.ToString(), descending));
            return this;
         }
         return this;
      }

      public ISqlReadMappedObject<TMapped> Map<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression<TMapped>(mapExpression);
         return new SqlReadMappedObject<TMapped>(Formatter, Connection, result.BinderExpression.Compile(), Descriptor, result, Joins);
      }

      private BinderExpressionCompiler.CompileResult<TMapped> PrepareMapExpression<TMapped>(LambdaExpression mapExpression)
      {
         var compiler = new BinderExpressionCompiler(Formatter);
         var result = compiler.Compile<TMapped>(mapExpression, Joins);
         Descriptor.AddJoins(result.Joins);
         Descriptor.SelectColumns.AddRange(result.Columns.Select(c => new QueryDescriptor.SelectColumnDescriptor(c.Sql, c.Alias)).Distinct());
         Descriptor.EnsureSelectColumnsAreAliased();
         return result;
      }


      public virtual new ISqlReadTableObject<T> Distinct()
      {
         return (ISqlReadTableObject<T>)base.Distinct();
      }

      public virtual new ISqlReadTableObject<T> Skip(int skip)
      {
         return (ISqlReadTableObject<T>)base.Skip(skip);
      }

      public virtual new ISqlReadTableObject<T> Take(int take)
      {
         return (ISqlReadTableObject<T>)base.Take(take);
      }

      public virtual ISqlReadTableObject<T> OrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlReadTableObject<T>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlReadTableObject<T> OrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlReadTableObject<T>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlReadTableObject<T> AddOrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlReadTableObject<T>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlReadTableObject<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlReadTableObject<T>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public new virtual ISqlReadTableObject<T> Union(ISqlReadObject sqlReadObject)
      {
         return (ISqlReadTableObject<T>)base.Union(sqlReadObject);
      }

      public ISqlReadTableObject<T> GroupBy(params Expression<Func<T, object>>[] expressions)
      {
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionBuilder();
            var context = new QuobContext(Formatter, Joins);

            sqlBuilder.BuildSql(context, expression);
            Descriptor.AddJoins(context.QuobJoins);
            Descriptor.GroupByColumns.Add(context.ToString());
            return this;
         }
         return this;
      }

      public ISqlReadTableObject<T> Clone()
      {
         return (ISqlReadTableObject<T>)CloneSqlReadObject();
      }

      public ISqlReadTableObject<T> Where(Expression<Func<T, bool>> whereClause)
      {
         if (whereClause == null) return this;
         this.CastTo<ISqlReadObject>().Where(whereClause);
         return this;
      }

      public ISqlReadTableObject<T> Having(Expression<Func<T, bool>> havingClause)
      {
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new QuobContext(Formatter, Joins) { ArgumentsAsLiterals = false };

         sqlBuilder.BuildSql(context, havingClause);
         Descriptor.AddJoins(context.QuobJoins);
         Descriptor.HavingClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }


      protected override SqlReadObject CloneSqlReadObject()
      {
         //TODO: need to clone joins?
         return new SqlReadTableObject<T>(Connection) { Descriptor = Descriptor.Clone(), Joins = Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList() };
      }



   }
}