using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Core.Impl
{
   public class QuobImpl : IQuob, IAttachable
   {

      protected QuobImpl(ISqlResource sqlResource, QuobSession context)
         : this(sqlResource)
      {
         Context = context;
         if (context == null) throw new ArgumentNullException("context");
      }

      public QuobImpl(ISqlResource sqlResource)
      {
         if (sqlResource == null) throw new ArgumentNullException("sqlResource");
         SqlResource = sqlResource;
      }

      protected QuobSession Context { get; private set; }
      public ISqlResource SqlResource { get; private set; }

      protected virtual QuobImpl SelfOrNew(QuobSession context, ISqlResource sqlResource = null)
      {
         return (sqlResource == null && context == Context) ? this : new QuobImpl(sqlResource ?? SqlResource, context);
      }


      public virtual IQuob Where(Expression expression)
      {
         var context = Context ?? new QuobSession(this);
         var compileResult = SqlResource.BuildSql(expression,context.Arguments);
         context.WhereClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }

      public virtual IQuob Having(Expression expression)
      {
         var context = Context ?? new QuobSession(this);
         var compileResult = SqlResource.BuildSql(expression, context.Arguments);
         context.HavingClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }

      public IQuob Where(string expression)
      {
         var context = Context ?? new QuobSession(this);
         var compileResult = SqlResource.BuildSqlPredicate(expression, null,context.Arguments);
         context.WhereClauses.Add(compileResult.Sql);
         return SelfOrNew(context);
      }

      public IQuob Having(string expression)
      {
         var context = Context ?? new QuobSession(this);
         var compileResult = SqlResource.BuildSqlPredicate(expression, null, context.Arguments);
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
         var context = Context ?? new QuobSession(this);
         var sqlOrderExpression = SqlResource.BuildSqlOrderBy(descending,expressions);
         if (reset)
         {
            context.Order.Clear();
         }
         context.Order.Add(sqlOrderExpression);
         return SelfOrNew(context);
      }

      public virtual IQuob Skip(int? skip)
      {
         var context = Context ?? new QuobSession(this);
         context.Skip = skip;
         return SelfOrNew(context);
      }

      public virtual IQuob Take(int? take)
      {
         var context = Context ?? new QuobSession(this);
         context.Take = take;
         return SelfOrNew(context);
      }

      public virtual IQuob OrderBy(string expression)
      {
         var context = Context ?? new QuobSession(this);
         var sqlOrderExpression = SqlResource.BuildSqlOrderBy(expression, SqlResource.GetEntityType(context.DbSession));
         context.Order.Clear();
         context.Order.Add(sqlOrderExpression);
         return SelfOrNew(context);
      }


      public virtual IEnumerable<object> ToEnumerable()
      {
         var sql = SqlResource.BuildSqlSelect(Context.GetSqlTemplateArgs());
         return Context.DbSession.Query(sql, SqlResource.GetBinder(Context.DbSession), Context.GetArguments(), false);
      }
      public virtual IEnumerable<object> ToEnumerable(out int count)
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            SqlResource.GetBinder(Context.DbSession)
         };
         var reader = Context.DbSession.QueryMultiple(sql, binders, Context.GetArguments());
         count = reader.Read<int>().Single();
         return reader.Read<object>(false);
      }

      protected virtual string GetDuoSql()
      {
         var sql = SqlResource.BuildSqlCount(Context.GetSqlTemplateArgs())
                   + SqlResource.Dialect.StatementSeperator
                   + SqlResource.BuildSqlSelect(Context.GetSqlTemplateArgs());
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

      public virtual int TotalCount()
      {
         var sql = SqlResource.BuildSqlCount(Context.GetSqlTemplateArgs());
         return Context.DbSession.Query(sql, r => r.GetInt32(0), Context.GetArguments()).Single();
      }

      public virtual bool Exists()
      {
         var sql = string.Format(SqlResource.Dialect.ExistsFormat, SqlResource.BuildSqlCount(Context.GetSqlTemplateArgs()));
         return Context.DbSession.Query(sql, r => r.GetBoolean(0), Context.GetArguments()).Single();
      }

      public virtual async Task<List<object>> FetchAsync()
      {
         var sql = SqlResource.BuildSqlSelect(Context.GetSqlTemplateArgs());
         return await Context.DbSession.QueryAsync(sql, SqlResource.GetBinder(Context.DbSession), Context.GetArguments());
      }

      public virtual async Task<CollectionWithCountResult<object>> FetchWithCountAsync()
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            SqlResource.GetBinder(Context.DbSession)
         };
         var reader = await Context.DbSession.QueryMultipleAsync(sql, binders, Context.GetArguments());
         var count = (await reader.ReadAsync<int>()).Single();
         var collection = await reader.ReadAsync();
         return new CollectionWithCountResult<object>
         {
            Collection = collection,
            TotalCount = count
         };
      }

      public virtual async Task<int> TotalCountAsync()
      {
         var sql = SqlResource.BuildSqlCount(Context.GetSqlTemplateArgs());
         return (await Context.DbSession.QueryAsync(sql, r => r.GetInt32(0), Context.GetArguments())).Single();
      }

      public virtual async Task<bool> ExistsAsync()
      {
         var sql = string.Format(SqlResource.Dialect.ExistsFormat, SqlResource.BuildSqlCount(Context.GetSqlTemplateArgs()));
         return (await Context.DbSession.QueryAsync(sql, r => r.GetBoolean(0), Context.GetArguments())).Single();
      }

      public virtual IQuob Select(string expression)
      {
         return SelfOrNew(Context.Clone(), SqlResource.Map(expression, SqlResource.GetEntityType(Context.DbSession)));
      }

      public virtual IQuob Select(LambdaExpression expression)
      {
         return SelfOrNew(Context.Clone(),SqlResource.Map(expression));
      }

      public virtual IQuob Attach(IXAdoDbSession session)
      {
         var clone = SelfOrNew(new QuobSession(this) { DbSession = session });
         clone.SqlResource.GetBinder(session);
         return clone;
      }

   }
}
