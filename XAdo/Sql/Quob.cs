using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XAdo.Core.Interface;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   public class Quob<TEntity> : IQuob<TEntity>
   {

      private string 
         _sql;

      private ISqlDialect 
         _dialect;
      private SelectInfo 
         _selectInfo;
      private Func<IDataRecord, TEntity> 
         _binder;
      private QueryContext
         _context;
      private IDictionary<string, string> 
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
         VerifyCreateTemplate();
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
            VerifyCreateTemplate();
         }
         var target = CloneOrSelf();
         target._context.Session = session;
         return target;
      }

      public virtual IQuob<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
      {
         if (predicate == null) throw new ArgumentNullException("predicate");
         var target = CloneOrSelf();
         var compileResult = new SqlBuilder(_dialect,"wp_").Parse(predicate,_map);
         target._context.Where = compileResult.Sql;
         target._context.WhereArguments = compileResult.Arguments;
         return target;

      }
      public virtual IQuob<TEntity> Having(Expression<Func<TEntity, bool>> predicate)
      {
         if (predicate == null) throw new ArgumentNullException("predicate");
         var target = CloneOrSelf();
         var compileResult = new SqlBuilder(_dialect, "hp_").Parse(predicate, _map);
         target._context.Having = compileResult.Sql;
         target._context.HavingArguments = compileResult.Arguments;
         return target;
      }
      public virtual IQuob<TEntity> Skip(int? skip)
      {
         var target = CloneOrSelf();
         target._context.SkipValue = skip;
         return target;
      }
      public virtual IQuob<TEntity> Take(int? take)
      {
         var target = CloneOrSelf();
         target._context.TakeValue = take;
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
         var sql = _sql.FormatSqlTemplate(_context);
         return _context.Session.Query(sql, _binder, _context.GetArguments(), false);
      }
      public virtual IEnumerable<TEntity> ToEnumerable(out int count)
      {
         var sql =
            _dialect.CountFormat.FormatWith(_sqlCount).FormatSqlTemplate(_context.Clone(true))
            + Environment.NewLine 
            + _dialect.StatementSeperator 
            + _sql.FormatSqlTemplate(_context);
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
         var sql =
            _dialect.CountFormat.FormatWith(_sqlCount).FormatSqlTemplate(_context.Clone(true))
            + Environment.NewLine
            + _dialect.StatementSeperator
            + _sql.FormatSqlTemplate(_context);
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

      private void Initialize()
      {
         _selectInfo = new SqlSelectParser().Parse(_sql);
         _sql = _selectInfo.Sql;
         _binder = _selectInfo.BuildFactory<TEntity>().Compile();
         _map = new ReadOnlyDictionary<string,string>(
            _selectInfo.Columns.ToDictionary(m => (m.Path + "." + m.Name).TrimStart('.'), m => m.Expression,StringComparer.OrdinalIgnoreCase)
          );
         _sqlCount = _selectInfo.AsInnerQuery();
      }
      private Quob<TEntity> CloneOrSelf()
      {
         return _context != null ? this : new Quob<TEntity>(true)
         {
            _sql = _sql,
            _sqlCount = _sqlCount,
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
            _context.OrderColumns.Clear();
         }
         var sb = new StringBuilder();
         foreach (var e in expressions)
         {
            sb.Length = 0;
            var m = (MemberExpression)e.Body.Trim();
            m.IsParameterDependent(sb);
            var path = sb.ToString();
            var info = _selectInfo.Columns.Single(c => c.FullName.Equals(path,StringComparison.OrdinalIgnoreCase));//TODO: reconsider case insensitve compares, move to _selectinfo?
            _context.OrderColumns.Add(string.Format("{0} {1}", info.IsAggregate ? info.Alias : info.Expression, order));
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
      private void VerifyCreateTemplate()
      {
         if (_templateCreated || _dialect == null)
         {
            return;
         }
         _templateCreated = true;
         if (_sql.Contains("--$") || _sql.Contains("-- $"))
         {
            //if any custom placeholder exist then do not create the default template
            return;
         }
         var template = _dialect.SelectTemplate;
         template = Regexes.RegexSelect.Replace(template, m => _sql);
         template = Regexes.RegexSelectColumns.Replace(template, m =>  _sql.Substring(0, _selectInfo.FromPosition));
         template = Regexes.RegexFrom.Replace(template, m => _sql.Substring(_selectInfo.FromPosition));
         _sql = template;
      }
   }

   internal class Regexes
   {
      public static Regex
         RegexSelect = new Regex(@"\$\(SELECT\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
         RegexSelectColumns = new Regex(@"\$\(SELECT-COLUMNS\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
         RegexFrom = new Regex(@"\$\(FROM\.\.\.\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
   }
}
