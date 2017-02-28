using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;

namespace XAdo.Quobs
{
   public class Quob : IQuob, IAttachable
   {

      protected Quob(IQueryBuilder queryBuilder, QuobContext context)
         : this(queryBuilder)
      {
         Context = context;
         if (context == null) throw new ArgumentNullException("context");
      }

      public Quob(IQueryBuilder queryBuilder)
      {
         if (queryBuilder == null) throw new ArgumentNullException("queryBuilder");
         QueryBuilder = queryBuilder;
      }

      protected QuobContext Context { get; private set; }
      protected IQueryBuilder QueryBuilder { get; private set; }

      protected virtual Quob SelfOrNew(QuobContext context, IQueryBuilder querybuilder = null)
      {
         return (querybuilder == null && context == Context) ? this : new Quob(querybuilder ?? QueryBuilder, context);
      }


      public virtual IQuob Where(Expression expression)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         var compileResult = QueryBuilder.BuildSqlByExpression(expression,context.Arguments);
         context.WhereClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }

      public virtual IQuob Having(Expression expression)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         var compileResult = QueryBuilder.BuildSqlByExpression(expression, context.Arguments);
         context.HavingClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }

      public IQuob Where(string expression)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         var compileResult = QueryBuilder.BuildSqlPredicate(expression, null,context.Arguments);
         context.WhereClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }

      public IQuob Having(string expression)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         var compileResult = QueryBuilder.BuildSqlPredicate(expression, null, context.Arguments);
         context.HavingClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }


      public virtual IQuob OrderBy(params Expression[] expressions)
      {
         return OrderBy(false, true, expressions);
      }

      public virtual IQuob OrderByDescending(params Expression[] expressions)
      {
         return OrderBy(true, true, expressions);
      }

      public virtual IQuob AddOrderBy(params Expression[] expressions)
      {
         return OrderBy(false, false, expressions);
      }

      public virtual IQuob AddOrderByDescending(params Expression[] expressions)
      {
         return OrderBy(true, false, expressions);
      }

      private IQuob OrderBy(bool descending, bool reset, params Expression[] expressions)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         var sqlOrderExpression = QueryBuilder.BuildSqlOrderBy(descending,expressions);
         if (reset)
         {
            context.Order.Clear();
         }
         context.Order.Add(sqlOrderExpression);
         return SelfOrNew(context);
      }

      public virtual IQuob Skip(int? skip)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         context.Skip = skip;
         return SelfOrNew(context);
      }

      public virtual IQuob Take(int? take)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         context.Take = take;
         return SelfOrNew(context);
      }

      public virtual IQuob OrderBy(string expression)
      {
         var context = Context ?? new QuobContext(QueryBuilder.Dialect);
         var sqlOrderExpression = QueryBuilder.BuildSqlOrderBy(expression,null);
         context.Order.Clear();
         context.Order.Add(sqlOrderExpression);
         return SelfOrNew(context);
      }


      public virtual IEnumerable<object> ToEnumerable()
      {
         var sql = QueryBuilder.Format(Context.GetSqlTemplateArgs());
         return Context.Session.Query(sql, QueryBuilder.GetBinder(Context.Session), Context.GetArguments(), false);
      }
      public virtual IEnumerable<object> ToEnumerable(out int count)
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            QueryBuilder.GetBinder(Context.Session)
         };
         var reader = Context.Session.QueryMultiple(sql, binders, Context.GetArguments());
         count = reader.Read<int>().Single();
         return reader.Read<object>(false);
      }

      protected virtual string GetDuoSql()
      {
         var sql = QueryBuilder.AsCountQuery().Format(Context.Clone(true).GetSqlTemplateArgs())
                   + QueryBuilder.Dialect.StatementSeperator
                   + QueryBuilder.Format(Context.GetSqlTemplateArgs());
         return sql;
      }

      public virtual List<object> Fetch()
      {
         return ToEnumerable().ToList();
      }

      public virtual List<object> Fetch(out int count)
      {
         return ToEnumerable(out count).ToList();
      }

      public virtual object[] FetchToArray()
      {
         return ToEnumerable().ToArray();
      }

      public virtual object[] FetchToArray(out int count)
      {
         return ToEnumerable(out count).ToArray();
      }

      public virtual int Count()
      {
         var sql = QueryBuilder.AsCountQuery().Format(Context.Clone(true).GetSqlTemplateArgs());
         return Context.Session.Query(sql, r => r.GetInt32(0), Context.GetArguments()).Single();
      }

      public virtual bool Exists()
      {
         var sql = string.Format(QueryBuilder.Dialect.ExistsFormat, QueryBuilder.AsCountQuery().Format(Context.Clone(true).GetSqlTemplateArgs()));
         return Context.Session.Query(sql, r => r.GetBoolean(0), Context.GetArguments()).Single();
      }

      public virtual async Task<List<object>> FetchAsync()
      {
         var sql = QueryBuilder.Format(Context.GetSqlTemplateArgs());
         return await Context.Session.QueryAsync(sql, QueryBuilder.GetBinder(Context.Session), Context.GetArguments());
      }

      public virtual async Task<CollectionWithCountResult<object>> FetchWithCountAsync()
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            QueryBuilder.GetBinder(Context.Session)
         };
         var reader = await Context.Session.QueryMultipleAsync(sql, binders, Context.GetArguments());
         var count = (await reader.ReadAsync<int>()).Single();
         var collection = await reader.ReadAsync();
         return new CollectionWithCountResult<object>
         {
            Collection = collection,
            TotalCount = count
         };
      }

      public virtual async Task<int> CountAsync()
      {
         var sql = QueryBuilder.AsCountQuery().Format(Context.Clone(true).GetSqlTemplateArgs());
         return (await Context.Session.QueryAsync(sql, r => r.GetInt32(0), Context.GetArguments())).Single();
      }

      public virtual async Task<bool> ExistsAsync()
      {
         var sql = string.Format(QueryBuilder.Dialect.ExistsFormat, QueryBuilder.AsCountQuery().Format(Context.Clone(true).GetSqlTemplateArgs()));
         return (await Context.Session.QueryAsync(sql, r => r.GetBoolean(0), Context.GetArguments())).Single();
      }

      public virtual IQuob Select(string expression)
      {
         return SelfOrNew(Context.Clone(), QueryBuilder.Map(expression, QueryBuilder.GetBinderType(Context.Session)));
      }

      public virtual IQuob Select(LambdaExpression expression)
      {
         return SelfOrNew(Context.Clone(),QueryBuilder.Map(expression));
      }

      public virtual IQuob Attach(IAdoSession session)
      {
         var clone = SelfOrNew(new QuobContext(QueryBuilder.Dialect) { Session = session });
         clone.QueryBuilder.GetBinder(session);
         return clone;
      }

   }
}
