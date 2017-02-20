using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XAdo.Core;
using XAdo.Core.Interface;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   public class Quob<TEntity> : IQuob<TEntity>, IQuob, IAttachable
   {

      #region Types
      private class MapFactory<TMapped> : IMappedQuobFactory
      {
         private Quob<TEntity> _quob;

         public IMappedQuobFactory SetRequestor(IQuob quob)
         {
            _quob = quob.CastTo<Quob<TEntity>>();
            return this;
         }

         public IQuob CreateMappedQuob(Expression mappedBinder, SqlSelectInfo mappedSelectInfo)
         {
            var mapped = new Quob<TMapped>(true)
            {
               _sql = mappedSelectInfo.Sql,
               _dialect = _quob._dialect,
               _selectInfo = mappedSelectInfo,
               _binderInfo = new BinderInfo(mappedBinder),
               _binder = mappedBinder.CastTo<Expression<Func<IDataRecord, TMapped>>>().CompileCached(),
               _map = new ReadOnlyDictionary<string, ColumnInfo>(
                  mappedSelectInfo.Columns.Where(m => m.Name != null).ToDictionary(m => (m.Path + (m.Path.Length > 0 ? "." : "") + m.Name), m => m,
                     StringComparer.OrdinalIgnoreCase)
                  ),
               _sqlCount = mappedSelectInfo.AsInnerQuery()
            };
            return mapped;
         }

      }

      #endregion

      private static LRUCache<string, object> _mappedCache = new LRUCache<string, object>(50);


      private string 
         _sql;

      private ISqlDialect 
         _dialect;
      private SqlSelectInfo 
         _selectInfo;
      private BinderInfo
         _binderInfo;

      private Func<IDataRecord, TEntity> 
         _binder;
      private QueryContext
         _context;
      private IDictionary<string, ColumnInfo> 
         _map;
      private string 
         _sqlCount;

      private Quob(bool localonly)
      {
         
      }

      public Quob(ISqlDialect dialect = null)
      {
         var att = typeof(TEntity).GetAnnotation<SqlSelectAttribute>();
         //todo: add convention based sql generator
         if (att == null)
         {
            throw new Exception("No SqlSelectAttribute annotation found for type " + typeof(TEntity).Name);
         }
         _sql = att.SqlSelect;
         _dialect = dialect;
         Initialize();
      }
      public Quob(string sqlSelect, ISqlDialect dialect = null)
      {
         _sql = sqlSelect;
         _dialect = dialect;
         Initialize();
      }

      public virtual Quob<TEntity> Attach(IAdoSession session)
      {
         _context = null;
         if (_dialect == null)
         {
            _dialect = session.Context.GetInstance<ISqlDialect>();
            EnsureTemplated();
         }
         var target = EnsureContext();
         target._context.Session = session;
         return target;
      }

      public virtual IQuob<TMapped> Select<TMapped>(Expression<Func<TEntity, TMapped>> binder)
      {
         var mapped = (Quob<TMapped>)_mappedCache.GetOrAdd(binder.GetKey(), t =>
         {
            SqlSelectInfo mappedSelectInfo;
            var mappedBinder = (Expression<Func<IDataRecord, TMapped>>)_binderInfo.Map(binder, _selectInfo, out mappedSelectInfo);
            return
               new MapFactory<TMapped>().SetRequestor(this)
                  .CreateMappedQuob(mappedBinder, mappedSelectInfo)
                  .CastTo<Quob<TMapped>>();
         }); 
         mapped.Attach(_context.Session);
         mapped._context = _context.Clone();
         return mapped;
      }
      public virtual IQuob Select(params string[] columns)
      {
         if (columns.Length == 1)
         {
            columns = columns[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().Replace('/','.')).ToArray();
         }

         var mapped = (IAttachable)_mappedCache.GetOrAdd(string.Join(":", columns), t =>
         {
            var map = typeof (TEntity).GetFullNameToMemberMap();
            string current = null;
            Type type;
            try
            {
               type = AnonymousTypeHelper.GetOrCreateType(columns,
                  columns.Select(c => map[current = c].GetMemberType()).ToList());
            }
            catch (KeyNotFoundException ex)
            {
               throw new Exception("The member name '" + current + "' is invalid");
            }
            var selectInfo = _selectInfo.Map(columns);
            var binder = selectInfo.CreateBinder(type);
            return typeof (MapFactory<>)
               .MakeGenericType(typeof (TEntity), type)
               .CreateInstance()
               .CastTo<IMappedQuobFactory>()
               .SetRequestor(this)
               .CreateMappedQuob(binder, selectInfo);
         });
         mapped.Attach(_context.Session);
         mapped.SetQueryContext(_context.Clone());
         return mapped.CastTo<IQuob>();
      }

      public virtual IQuob<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
      {
         if (predicate == null) throw new ArgumentNullException("predicate");
         var target = EnsureContext();
         var compileResult = new SqlBuilder(_dialect, "p_").Parse(predicate, _map, target._context.Arguments);
         target._context.WhereClauses.Add(compileResult.Sql);
         return target;

      }
      public virtual IQuob<TEntity> Having(Expression<Func<TEntity, bool>> predicate)
      {
         if (predicate == null) throw new ArgumentNullException("predicate");
         var target = EnsureContext();
         var compileResult = new SqlBuilder(_dialect, "hp_").Parse(predicate, _map, target._context.Arguments);
         target._context.HavingClauses.Add(compileResult.Sql);
         return target;
      }
      public virtual IQuob<TEntity> Skip(int? skip)
      {
         var target = EnsureContext();
         target._context.Skip = skip;
         return target;
      }
      public virtual IQuob<TEntity> Take(int? take)
      {
         var target = EnsureContext();
         target._context.Take = take;
         return target;
      }

      public virtual IQuob<TEntity> OrderBy(params Expression<Func<TEntity, object>>[] expressions)
      {
         return BuildOrderBySql(true,null,expressions);
      }
      public virtual IQuob<TEntity> OrderByDescending(params Expression<Func<TEntity, object>>[] expressions)
      {
         return BuildOrderBySql(true,"DESC", expressions);
      }
      public virtual IQuob<TEntity> AddOrderBy(params Expression<Func<TEntity, object>>[] expressions)
      {
         return BuildOrderBySql(false,null, expressions);
      }
      public virtual IQuob<TEntity> AddOrderByDescending(params Expression<Func<TEntity, object>>[] expressions)
      {
         return BuildOrderBySql(false,"DESC", expressions);
      }

      public virtual IEnumerable<TEntity> ToEnumerable()
      {
         var sql = _sql.FormatSqlTemplate(_context.GetSqlTemplateArgs());
         return _context.Session.Query(sql, _binder, _context.GetArguments(), false);
      }
      public virtual IEnumerable<TEntity> ToEnumerable(out int count)
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            _binder
         };
         var reader = _context.Session.QueryMultiple(sql, binders,_context.GetArguments());
         count = reader.Read<int>().Single();
         return reader.Read<TEntity>(false);
      }

      public virtual List<TEntity> ToList()
      {
         return ToEnumerable().ToList();
      }
      public virtual List<TEntity> ToList(out int count)
      {
         return ToEnumerable(out count).ToList();
      }
      public virtual TEntity[] ToArray()
      {
         return ToEnumerable().ToArray();
      }
      public virtual TEntity[] ToArray(out int count)
      {
         return ToEnumerable(out count).ToArray();
      }
      public virtual IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector)
      {
         return ToEnumerable().ToDictionary(keySelector, elementSelector);
      }
      public virtual IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector, out int count)
      {
         return ToEnumerable(out count).ToDictionary(keySelector, elementSelector);
      }
      public virtual IDictionary<TKey, List<TValue>> ToGroupedList<TKey, TValue>(Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector)
      {
         return ToGroupedList(ToEnumerable(), groupKeySelector, listElementSelector);
      }
      public virtual int Count()
      {
         var sql = _dialect.CountFormat.FormatWith(_sqlCount).FormatSqlTemplate(_context.Clone(true));
         return _context.Session.Query(sql, r => r.GetInt32(0), _context.GetArguments()).Single();
      }
      public virtual bool Exists()
      {
         var sql = string.Format(_dialect.ExistsFormat, _selectInfo.AsInnerQuery().FormatSqlTemplate(_context.Clone(true)));
         return _context.Session.Query(sql, r => r.GetBoolean(0), _context.GetArguments()).Single();
      }

      public virtual async Task<List<TEntity>> ToListAsync()
      {
         var sql = _sql.FormatSqlTemplate(_context);
         return await _context.Session.QueryAsync(sql, _binder, _context.GetArguments());
      }
      public virtual async Task<AsyncCountListResult<TEntity>> ToCountListAsync()
      {
         var sql = GetDuoSql();
         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            _binder
         };
         var reader = await _context.Session.QueryMultipleAsync(sql, binders, _context.GetArguments());
         var count = (await reader.ReadAsync<int>()).Single();
         var collection = await reader.ReadAsync<TEntity>();
         return new AsyncCountListResult<TEntity>
         {
            Collection = collection,
            TotalCount = count
         };
      }
      public virtual async Task<IDictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> elementSelector)
      {
         return (await ToListAsync()).ToDictionary(keySelector, elementSelector);
      }
      public virtual async Task<IDictionary<TKey, List<TValue>>> ToGroupedListAsync<TKey, TValue>(Func<TEntity, TKey> groupKeySelector, Func<TEntity, TValue> listElementSelector)
      {
         return ToGroupedList((await ToListAsync()), groupKeySelector, listElementSelector);
      }
      public virtual async Task<int> CountAsync()
      {
         var sql = _dialect.CountFormat.FormatWith(_sqlCount).FormatSqlTemplate(_context.Clone(true));
         return (await _context.Session.QueryAsync(sql, r => r.GetInt32(0), _context.GetArguments())).Single();
      }
      public virtual async Task<bool> ExistsAsync()
      {
         var sql = _sqlCount.FormatSqlTemplate(_context.Clone(true));
         return (await _context.Session.QueryAsync(sql, r => r.GetBoolean(0), _context.GetArguments())).Single();
      }

      #region IQuob
      IQuob IQuob.Where(Expression expression)
      {
         return Where(expression.CastTo<Expression<Func<TEntity, bool>>>()).CastTo<IQuob>();
      }

      IQuob IQuob.Having(Expression expression)
      {
         return Having(expression.CastTo<Expression<Func<TEntity, bool>>>()).CastTo<IQuob>();
      }

      IQuob IQuob.Skip(int? skip)
      {
         return Skip(skip).CastTo<IQuob>();
      }

      IQuob IQuob.Take(int? take)
      {
         return Take(take).CastTo<IQuob>();
      }

      IQuob IQuob.OrderBy(params string[] expressions)
      {
         if (expressions.Length == 1)
         {
            expressions =
               expressions[0].Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var first = true;
            foreach (var e in expressions)
            {
               var desc = e.StartsWith("-") || e.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);
               BuildOrderBySql(first, desc ? "DESC" : null, e.TrimStart('-').Split(' ').First().Replace('/', '.'));
               first = false;
            }
            return this;
         }

         return BuildOrderBySql(true, null, expressions);
      }

      IQuob IQuob.OrderByDescending(params string[] expressions)
      {
         return BuildOrderBySql(true, "DESC", expressions);
      }

      IQuob IQuob.AddOrderBy(params string[] expressions)
      {
         return BuildOrderBySql(false, null, expressions);
      }

      IQuob IQuob.AddOrderByDescending(params string[] expressions)
      {
         return BuildOrderBySql(false, "DESC", expressions);
      }

      IEnumerable<object> IQuob.ToEnumerable()
      {
         return ToEnumerable().Cast<object>();
      }

      IEnumerable<object> IQuob.ToEnumerable(out int count)
      {
         return ToEnumerable(out count).Cast<object>();
      }

      List<object> IQuob.ToList()
      {
         return ToEnumerable().Cast<object>().ToList();
      }

      List<object> IQuob.ToList(out int count)
      {
         return ToEnumerable(out count).Cast<object>().ToList();
      }

      object[] IQuob.ToArray()
      {
         return ToEnumerable().Cast<object>().ToArray();
      }

      object[] IQuob.ToArray(out int count)
      {
         return ToEnumerable(out count).Cast<object>().ToArray();
      }

      async Task<List<object>> IQuob.ToListAsync()
      {
         var sql = _sql.FormatSqlTemplate(_context);
         return await _context.Session.QueryAsync(sql, r => (object)_binder(r), _context.GetArguments());
      }

      async Task<AsyncCountListResult<object>> IQuob.ToCountListAsync()
      {
         var sql = GetDuoSql();

         var binders = new List<Delegate>
         {
            new Func<IDataRecord, int>(r => r.GetInt32(0)), 
            new Func<IDataRecord, object>(r => (object)_binder(r))
         };

         var reader = await _context.Session.QueryMultipleAsync(sql, binders, _context.GetArguments());
         var count = (await reader.ReadAsync<int>()).Single();
         var collection = await reader.ReadAsync<object>();
         return new AsyncCountListResult<object>
         {
            Collection = collection,
            TotalCount = count
         };

      }

      #region Is implemented by IQuob<TEntity>
      //int IQuob.Count()
      //{
      //   throw new NotImplementedException();
      //}

      //bool IQuob.Exists()
      //{
      //   throw new NotImplementedException();
      //}

      //Task<int> IQuob.CountAsync()
      //{
      //   throw new NotImplementedException();
      //}

      //Task<bool> IQuob.ExistsAsync()
      //{
      //   throw new NotImplementedException();
      //}
      #endregion

      #endregion

      #region IAttachable
      void IAttachable.Attach(IAdoSession session)
      {
         Attach(session);
      }
      void IAttachable.SetQueryContext(QueryContext context)
      {
         SetContext(context);
      }
      protected virtual void SetContext(QueryContext context)
      {
         _context = context;
      }
      #endregion

      private void Initialize()
      {
         _selectInfo = new SqlSelectParser().Parse(_sql);
         _sql = _selectInfo.Sql;
         var binderExpression = _selectInfo.CreateBinder<TEntity>();
         _binder = binderExpression.Compile();
         _binderInfo = new BinderInfo(binderExpression);
         _map = new ReadOnlyDictionary<string,ColumnInfo>(
            _selectInfo.Columns.ToDictionary(m => (m.Path + "." + m.Name).TrimStart('.'), m => m,StringComparer.OrdinalIgnoreCase)
          );
         _sqlCount = _selectInfo.AsInnerQuery();
         EnsureTemplated();
      }
      private Quob<TEntity> EnsureContext()
      {
         return _context != null ? this : new Quob<TEntity>(true)
         {
            _sql = _sql,
            _sqlCount = _sqlCount,
            _binderInfo = _binderInfo,
            _binder = _binder,
            _context = new QueryContext(_dialect),
            _dialect = _dialect,
            _selectInfo = _selectInfo,
            _map = _map
         };
      }
      private Quob<TEntity> BuildOrderBySql(bool reset, string order, params Expression<Func<TEntity, object>>[] expressions)
      {
         if (reset)
         {
            _context.Order.Clear();
         }
         var path = new StringBuilder();
         foreach (var e in expressions)
         {
            path.Length = 0;
            var m = (MemberExpression)e.Body.Trim();
            m.IsParameterDependent(path);
            var info = _selectInfo.FindColumn(path.ToString());
            _context.Order.Add(string.Format("{0} {1}", info.IsCalculated ? info.Alias : info.Expression, order));
         }
         return this;
      }
      private IQuob BuildOrderBySql(bool reset, string order, params string[] expressions)
      {
         if (reset)
         {
            _context.Order.Clear();
         }
         foreach (var path in expressions)
         {
            var info = _selectInfo.FindColumn(path);
            if (info == null)
            {
               throw new Exception("The sort member '"+path+"' is invalid");
            }
            _context.Order.Add(string.Format("{0} {1}", info.IsCalculated ? info.Alias : info.Expression, order));
         }
         return this;
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
                     throw new InvalidOperationException("You need to order by key first, before calling ToGroupedList", ex);
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
               throw new InvalidOperationException("You need to order by key first, before calling ToGroupedList", ex);
            }
         }
         return dictionary;
      }

      private bool _templateCreated;
      private void EnsureTemplated()
      {
         if (_templateCreated || _dialect == null)
         {
            return;
         }
         _templateCreated = true;
         _sql = CreateTemplate(_sql);
      }
      private string CreateTemplate(string sql)
      {
         if (sql.Contains("--$") || sql.Contains("-- $"))
         {
            //if any custom placeholder exist then do not create the default template
            return sql;
         }
         var template = _dialect.SelectTemplate;
         template = Regexes.RegexSelect.Replace(template, m => sql);
         template = Regexes.RegexSelectColumns.Replace(template, m => sql.Substring(0, _selectInfo.FromPosition));
         template = Regexes.RegexFrom.Replace(template, m => sql.Substring(_selectInfo.FromPosition));
         return template;
      }
      private string GetDuoSql()
      {
         var sql =
            _dialect.CountFormat.FormatWith(_sqlCount).FormatSqlTemplate(_context.Clone(true).GetSqlTemplateArgs())
            + Environment.NewLine
            + _dialect.StatementSeperator
            + _sql.FormatSqlTemplate(_context.GetSqlTemplateArgs());
         return sql;

      }

   }

   internal class Regexes
   {
      // moved these fields outside the generic class
      public static Regex
         RegexSelect = new Regex(@"\$\(SELECT\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
         RegexSelectColumns = new Regex(@"\$\(SELECT-COLUMNS\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
         RegexFrom = new Regex(@"\$\(FROM\.\.\.\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
   }
}
