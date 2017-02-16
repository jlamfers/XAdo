using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace XAdo.Sql
{
   public interface IQuob<TEntity>
   {
      IQuob<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
      IQuob<TEntity> Having(Expression<Func<TEntity, bool>> predicate);
      IQuob<TEntity> Skip(int? skip);
      IQuob<TEntity> Take(int? take);
      IQuob<TEntity> OrderBy(params Expression<Func<TEntity, object>>[] expressions);
      IQuob<TEntity> OrderByDescending(params Expression<Func<TEntity, object>>[] expressions);
      IQuob<TEntity> AddOrderBy(params Expression<Func<TEntity, object>>[] expressions);
      IQuob<TEntity> AddOrderByDescending(params Expression<Func<TEntity, object>>[] expressions);

      IEnumerable<TEntity> ToEnumerable();
      IEnumerable<TEntity> ToEnumerable(out int count);

      List<TEntity> ToList();
      List<TEntity> ToList(out int count);
      TEntity[] ToArray();
      TEntity[] ToArray(out int count);
      IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector);
      IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector, out int count);
      IDictionary<TKey, List<TValue>> ToGroupedList<TKey, TValue>(Func<TEntity, TKey> groupKeySelector,Func<TEntity, TValue> listElementSelector);
      int Count();
      bool Exists();

      // async
      Task<List<TEntity>> ToListAsync();
      Task<AsyncCountListResult<TEntity>> ToCountListAsync();
      Task<IDictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector);
      Task<IDictionary<TKey, List<TValue>>> ToGroupedListAsync<TKey, TValue>(Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector);
      Task<int> CountAsync();
      Task<bool> ExistsAsync();
      IQuob<TMapped> Select<TMapped>(Expression<Func<TEntity, TMapped>> binder);
      IQuob Select(params string[] columns);
   }

   public interface IQuob
   {
      IQuob Where(Expression expression);
      IQuob Having(Expression expression);
      IQuob Skip(int? skip);
      IQuob Take(int? take);
      IQuob OrderBy(params string[] expressions);
      IQuob OrderByDescending(params string[] expressions);
      IQuob AddOrderBy(params string[] expressions);
      IQuob AddOrderByDescending(params string[] expressions);

      IEnumerable<object> ToEnumerable();
      IEnumerable<object> ToEnumerable(out int count);

      List<object> ToList();
      List<object> ToList(out int count);
      object[] ToArray();
      object[] ToArray(out int count);
      int Count();
      bool Exists();

      // async
      Task<List<object>> ToListAsync();
      Task<AsyncCountListResult<object>> ToCountListAsync();
      Task<int> CountAsync();
      Task<bool> ExistsAsync();
   }
}