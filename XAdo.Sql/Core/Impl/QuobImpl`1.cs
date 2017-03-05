using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Core.Impl
{
   public class QuobImpl<TEntity> : QuobImpl, IQuob<TEntity>
   {
      protected QuobImpl(ISqlResource<TEntity> queryBuilder, QuobSession context) : base(queryBuilder, context)
      {
      }

      public QuobImpl(ISqlResourceRepository factory)
         : this(factory.Get<TEntity>())
      {
      }

      public QuobImpl(ISqlResource<TEntity> sqlResource)
         : base(sqlResource)
      {
      }

      protected override QuobImpl SelfOrNew(QuobSession context, ISqlResource querybuilder = null)
      {
         return (Context == context && querybuilder == null) ? this : new QuobImpl<TEntity>((ISqlResource<TEntity>)(querybuilder ?? SqlResource), context);
      }

      public IQuob<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
      {
         return base.Where(predicate).CastTo<IQuob<TEntity>>();
      }

      public IQuob<TEntity> Having(Expression<Func<TEntity, bool>> predicate)
      {
         return base.Having(predicate).CastTo<IQuob<TEntity>>();
      }

      public new IQuob<TEntity> Skip(int? skip)
      {
         return base.Skip(skip).CastTo<IQuob<TEntity>>();
      }

      public new IQuob<TEntity> Take(int? take)
      {
         return base.Take(take).CastTo<IQuob<TEntity>>();
      }

      public IQuob<TEntity> OrderBy(params Expression<Func<TEntity, object>>[] expressions)
      {
         return base.OrderBy(expressions.Cast<Expression>().ToArray()).CastTo<IQuob<TEntity>>();
      }

      public IQuob<TEntity> OrderByDescending(params Expression<Func<TEntity, object>>[] expressions)
      {
         return base.OrderByDescending(expressions.Cast<Expression>().ToArray()).CastTo<IQuob<TEntity>>();
      }

      public IQuob<TEntity> AddOrderBy(params Expression<Func<TEntity, object>>[] expressions)
      {
         return base.AddOrderBy(expressions.Cast<Expression>().ToArray()).CastTo<IQuob<TEntity>>();
      }

      public IQuob<TEntity> AddOrderByDescending(params Expression<Func<TEntity, object>>[] expressions)
      {
         return base.AddOrderByDescending(expressions.Cast<Expression>().ToArray()).CastTo<IQuob<TEntity>>();
      }

      public new IEnumerable<TEntity> ToEnumerable()
      {
         var sql = SqlResource.Format(Context.GetSqlTemplateArgs());
         return Context.Session.Query(sql, SqlResource.GetBinder<TEntity>(), Context.GetArguments(), false);
      }

      public new IEnumerable<TEntity> ToEnumerable(out int count)
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            SqlResource.GetBinder<TEntity>()
         };
         var reader = Context.Session.QueryMultiple(sql, binders, Context.GetArguments());
         count = reader.Read<int>().Single();
         return reader.Read<TEntity>(false);
      }

      public new List<TEntity> Fetch()
      {
         return ToEnumerable().ToList();
      }

      public new List<TEntity> Fetch(out int count)
      {
         return ToEnumerable(out count).ToList();
      }

      public new TEntity[] FetchToArray()
      {
         return ToEnumerable().ToArray();
      }

      public new TEntity[] FetchToArray(out int count)
      {
         return ToEnumerable(out count).ToArray();
      }

      public IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector)
      {
         return ToEnumerable().ToDictionary(keySelector, elementSelector);
      }

      public IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector, out int count)
      {
         return ToEnumerable(out count).ToDictionary(keySelector, elementSelector);
      }

      public IDictionary<TKey, List<TValue>> FetchToGroupedList<TKey, TValue>(Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector)
      {
         return ToGroupedList(ToEnumerable(), groupKeySelector, listElementSelector);
      }

      public async new Task<List<TEntity>> FetchAsync()
      {
         var sql = SqlResource.Format(Context.GetSqlTemplateArgs());
         return await Context.Session.QueryAsync(sql, SqlResource.GetBinder<TEntity>(), Context.GetArguments());
      }

      public new async Task<CollectionWithCountResult<TEntity>> FetchWithCountAsync()
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            SqlResource.GetBinder<TEntity>()
         };
         var reader = await Context.Session.QueryMultipleAsync(sql, binders, Context.GetArguments());
         var count = (await reader.ReadAsync<int>()).Single();
         var collection = await reader.ReadAsync<TEntity>();
         return new CollectionWithCountResult<TEntity>
         {
            Collection = collection,
            TotalCount = count
         };
      }

      public async Task<IDictionary<TKey, TValue>> FetchToDictionaryAsync<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector)
      {
         return (await FetchAsync()).ToDictionary(keySelector, elementSelector);
      }

      public async Task<IDictionary<TKey, List<TValue>>> FetchToGroupedListAsync<TKey, TValue>(Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector)
      {
         return ToGroupedList((await FetchAsync()), groupKeySelector, listElementSelector);
      }

      public IQuob<TMapped> Select<TMapped>(Expression<Func<TEntity, TMapped>> expression)
      {
         var qb = SqlResource.Map(expression).ToGeneric<TMapped>();
         return new QuobImpl<TMapped>(qb.ToGeneric<TMapped>(), Context.Clone());
      }

      private static IDictionary<TKey, List<TValue>> ToGroupedList<TKey, TValue>(IEnumerable<TEntity> enumerable, Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector)
      {
         var dictionary = new Dictionary<TKey, List<TValue>>();
         var current = default(TKey);
         List<TValue> list = null;
         foreach (var row in enumerable)
         {
            var key = groupKeySelector(row);
            if (!Equals(current, key))
            {
               if (list != null)
               {
                  try
                  {
                     dictionary.Add(current, list);
                  }
                  catch (ArgumentException ex)
                  {
                     throw new QuobException("You need to order by key first, before calling FetchToGroupedList", ex);
                  }
               }
               list = new List<TValue>();
               current = key;
            }
            var v = listElementSelector(row);
            if (!Equals(v, null) && list != null)
            {
               list.Add(v);
            }
         }
         if (list != null)
         {
            try
            {
               dictionary.Add(current, list);
            }
            catch (ArgumentException ex)
            {
               throw new QuobException("You need to order by key first, before calling FetchToGroupedList", ex);
            }
         }
         return dictionary;
      }

   }
}