using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace XAdo.Quobs.Core.Interface
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

      List<TEntity> Fetch();
      List<TEntity> Fetch(out int count);
      TEntity[] FetchToArray();
      TEntity[] FetchToArray(out int count);
      IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector);
      IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector, out int count);
      IDictionary<TKey, List<TValue>> FetchToGroupedList<TKey, TValue>(Func<TEntity, TKey> groupKeySelector,Func<TEntity, TValue> listElementSelector);

      int TotalCount();
      bool Exists();

      // async
      Task<List<TEntity>> FetchAsync();
      Task<CollectionWithCountResult<TEntity>> FetchWithCountAsync();
      Task<IDictionary<TKey, TValue>> FetchToDictionaryAsync<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector);
      Task<IDictionary<TKey, List<TValue>>> FetchToGroupedListAsync<TKey, TValue>(Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector);
      Task<int> TotalCountAsync();
      Task<bool> ExistsAsync();

      IQuob<TMapped> Select<TMapped>(Expression<Func<TEntity, TMapped>> expression);
      IQuob Select(string expression);
   }

   public interface IQuob
   {
      IQuob Where(Expression expression);
      IQuob Having(Expression expression);
      IQuob OrderBy(params Expression[] expressions);
      IQuob OrderByDescending(params Expression[] expressions);
      IQuob AddOrderBy(params Expression[] expressions);
      IQuob AddOrderByDescending(params Expression[] expressions);
      IQuob Skip(int? skip);
      IQuob Take(int? take);
      IQuob OrderBy(string expression);
      IQuob Where(string expression);
      IQuob Having(string expression);

      IEnumerable<object> ToEnumerable();
      IEnumerable<object> ToEnumerable(out int count);

      List<object> Fetch();
      List<object> Fetch(out int count);
      object[] FetchToArray();
      object[] FetchToArray(out int count);

      int TotalCount();
      bool Exists();

      // async
      Task<List<object>> FetchAsync();
      Task<CollectionWithCountResult<object>> FetchWithCountAsync();
      Task<int> TotalCountAsync();
      Task<bool> ExistsAsync();

      IQuob Select(string expression);
      IQuob Select(LambdaExpression expression);

      ISqlResource SqlResource { get; }
   }
}