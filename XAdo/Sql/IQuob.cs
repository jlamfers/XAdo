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
   }
}