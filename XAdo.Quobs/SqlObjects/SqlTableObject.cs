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
   public class SqlTableObject<T> : SqlFetchObject<T>, ISqlTableObject<T> 
      where T : IDbTable
   {

      public SqlTableObject(ISqlExecuter executer)
         : base(executer.GetSqlFormatter(), executer, new QueryDescriptor { FromTableName = typeof(T).GetTableDescriptor().Format(executer.GetSqlFormatter()) }, null)
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

      public ISqlMapObject<TMapped> Select<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression<TMapped>(mapExpression);
         return new SqlMapObject<TMapped>(Formatter, Executer, result.BinderExpression.Compile(), Descriptor, result, Joins);
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


      public virtual new ISqlTableObject<T> Distinct()
      {
         return (ISqlTableObject<T>)base.Distinct();
      }

      public virtual new ISqlTableObject<T> Skip(int skip)
      {
         return (ISqlTableObject<T>)base.Skip(skip);
      }

      public virtual new ISqlTableObject<T> Take(int take)
      {
         return (ISqlTableObject<T>)base.Take(take);
      }

      public virtual ISqlTableObject<T> OrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlTableObject<T>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlTableObject<T> OrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlTableObject<T>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlTableObject<T> AddOrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlTableObject<T>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlTableObject<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlTableObject<T>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public new virtual ISqlTableObject<T> Union(ISqlReadObject sqlReadObject)
      {
         return (ISqlTableObject<T>)base.Union(sqlReadObject);
      }

      public ISqlTableObject<T> GroupBy(params Expression<Func<T, object>>[] expressions)
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

      public ISqlTableObject<T> Clone()
      {
         return (ISqlTableObject<T>)CloneSqlReadObject();
      }

      public ISqlTableObject<T> Where(Expression<Func<T, bool>> whereClause)
      {
         if (whereClause == null) return this;
         this.CastTo<ISqlReadObject>().Where(whereClause);
         return this;
      }

      public ISqlTableObject<T> Having(Expression<Func<T, bool>> havingClause)
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
         return new SqlTableObject<T>(Executer) { Descriptor = Descriptor.Clone(), Joins = Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList() };
      }



   }
}