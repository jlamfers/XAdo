using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Core;
using XAdo.Core.Cache;
using XAdo.Core.Interface;
using XAdo.Sql.Core.Common;
using XAdo.Sql.Core.Linq;
using XAdo.Sql.Core.Mapper;
using XAdo.Sql.Core.Parser;
using XAdo.Sql.Core.Parser.Partials;
using XAdo.Sql.Dialects;

namespace XAdo.Sql.Core
{
   // immutable object
   public class QueryBuilder
   {

      public static QueryBuilder Parse(string sql, string template = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return QueryBuilderCache.GetOrAdd(Tuple.Create(sql,template), x =>
         {
            var parser = new SqlSelectParser();
            var partials = parser.Parse(x.CastTo<Tuple<string, string>>().Item1);
            var queryMap = new QueryBuilder(partials,template);
            return queryMap;
         });
      }

      #region Types
      private class MapVisitor : ExpressionVisitor
      {
         public class VisitResult
         {
            public List<Tuple<MetaColumnPartial, MemberInfo>> ToColumns { get; set; }
            public LambdaExpression ToExpression { get; set; }
         }
         private readonly IList<MetaColumnPartial> _fromColumns;
         private readonly IDictionary<MemberInfo, int> _fromIndices;
         private ParameterExpression _parameter;
         private List<Tuple<MetaColumnPartial, MemberInfo>> _toColumns;

         public MapVisitor(IList<MetaColumnPartial> fromColumns, IDictionary<MemberInfo, int> fromIndices)
         {
            _fromColumns = fromColumns;
            _fromIndices = fromIndices;
         }

         public VisitResult Substitute(LambdaExpression fromExpression)
         {
            _toColumns = new List<Tuple<MetaColumnPartial, MemberInfo>>();
            _parameter = Expression.Parameter(typeof(IDataRecord), "row");
            var body = Visit(fromExpression.Body);
            return new VisitResult
            {
               ToColumns = _toColumns,
               ToExpression = Expression.Lambda(body, _parameter)
            };
         }

         protected override Expression VisitNew(NewExpression node)
         {
            if (node.Arguments == null || !node.Arguments.Any())
            {
               return base.VisitNew(node);
            }
            var args = new List<Expression>();
            for (var i = 0; i < node.Arguments.Count; i++)
            {
               if (node.Arguments[i].NodeType == ExpressionType.New)
               {
                  args.Add(Visit(node.Arguments[i]));
                  continue;
               }
               var member = node.Arguments[i].GetMemberInfo(false);
               int index;
               if (member == null || !_fromIndices.TryGetValue(member, out index))
               {
                  args.Add(Visit(node.Arguments[i]));
                  continue;
               }

               var column = _fromColumns[index];
               //note: indices are reset later
               var newIndex = _toColumns.FindIndex(c => c.Item1.Index == column.Index);
               if (newIndex == -1)
               {
                  newIndex = _toColumns.Count;
                  var c = column.Clone();
                  _toColumns.Add(Tuple.Create(c, node.Members[i]));
               }
               args.Add(node.Members[i].GetDataRecordRecordGetterExpression(newIndex, _parameter, column.Meta.NotNull));
            }
            return Expression.New(node.Constructor, args, node.Members);
         }

         protected override MemberBinding VisitMemberBinding(MemberBinding node)
         {

            int index;
            if (_fromIndices.TryGetValue(node.Member, out index))
            {
               var column = _fromColumns[index];
               var newIndex = _toColumns.FindIndex(c => c.Item1.Index == column.Index);
               if (newIndex == -1)
               {
                  newIndex = _toColumns.Count;
                  var c = column.Clone();
                  _toColumns.Add(Tuple.Create(c, node.Member));
               }
               return node.Member.GetDataRecordMemberAssignmentExpression(newIndex, _parameter, column.Meta.NotNull);
            }
            return base.VisitMemberBinding(node);
         }

         protected override Expression VisitMember(MemberExpression node)
         {
            int index;
            if (_fromIndices.TryGetValue(node.Member, out index))
            {
               var column = _fromColumns[index];
               var newIndex = _toColumns.FindIndex(c => c.Item1.Index == column.Index);
               if (newIndex == -1)
               {
                  newIndex = _toColumns.Count;
                  var c = column.Clone();
                  _toColumns.Add(Tuple.Create(c, (MemberInfo)null));
               }
               return node.Member.GetDataRecordRecordGetterExpression(newIndex, _parameter, column.Meta.NotNull);
            }
            return base.VisitMember(node);
         }
      }
      private class BinderInfo
      {
         private interface ICompiler
         {
            Delegate Compile(LambdaExpression expression);
            Func<IDataRecord, object> CastToObjectBinder(Delegate @delegate);
         }
         private class Compiler<TEntity> : ICompiler
         {
            public Delegate Compile(LambdaExpression expression)
            {
               return expression.CastTo<Expression<Func<IDataRecord, TEntity>>>().CompileCached();
            }
            public Func<IDataRecord, object> CastToObjectBinder(Delegate @delegate)
            {
               var t = (Func<IDataRecord, TEntity>)@delegate;
               return r => t(r);
            }
         }
         private Delegate _binderDelegate;
         private Func<IDataRecord, object> _objectBinderDelegate;

         public LambdaExpression BinderExpression;
         public IDictionary<MemberInfo, int> MemberIndexMap;

         public Delegate BinderDelegate
         {
            get { return _binderDelegate ?? (_binderDelegate = typeof(Compiler<>)
               .MakeGenericType(BinderExpression.Body.Type)
               .CreateInstance()
               .CastTo<ICompiler>()
               .Compile(BinderExpression)); }
         }

         public Func<IDataRecord, object> ObjectBinderDelegate
         {
            get
            {
               {
                  return _objectBinderDelegate ?? (_objectBinderDelegate = typeof(Compiler<>)
               .MakeGenericType(BinderExpression.Body.Type)
               .CreateInstance()
               .CastTo<ICompiler>()
               .CastToObjectBinder(BinderDelegate));
               }
            }
         }
      }
      #endregion

      private static readonly HashSet<string> 
         DefaultAllowedAggregates = new HashSet<string>(new[]
         {
            "COUNT", "AVG", "MIN", "MAX", "SUM"
         }, 
         StringComparer.OrdinalIgnoreCase
      );


      protected static readonly LRUCache<object,QueryBuilder>
         QueryBuilderCache = new LRUCache<object, QueryBuilder>(1000);

      private ICache<Type, BinderInfo>
         _binderCache = new SmallCache<Type, BinderInfo>();

      private ICache<Type, QueryBuilder>
         _mapCache = new SmallCache<Type, QueryBuilder>();

      private string
         _formattedSql;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly MethodInfo 
         IsDbNull = MemberInfoFinder.GetMethodInfo<IDataRecord>(r => r.IsDBNull(0));

      private QueryBuilder
         _countQuery;

      #region Hidden fields
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private readonly IList<SqlPartial> _partials;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private WithPartial _with;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _withChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private SelectPartial _select;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private TablePartial _table;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IList<JoinPartial> _joins;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private WherePartial _where;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _whereChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private GroupByPartial _groupBy;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _groupbyChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private HavingPartial _having;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _havingChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private HavingPartial _orderBy;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _orderbyChecked;
      #endregion

      public QueryBuilder(IList<SqlPartial> partials, string template=null)
      {
         if (template == null)
         {
            _partials = partials as ReadOnlyCollection<SqlPartial> ?? partials.ToList().AsReadOnly();
         }
         else
         {
            _partials = partials.MergeTemplate(template).AsReadOnly();
         }
      }

      public WithPartial With
      {
         get
         {
            if (_withChecked) return _with;
            _withChecked = true;
            return _with ?? (_with = _partials.OfType<WithPartial>().SingleOrDefault());
         }
      }

      public SelectPartial Select
      {
         get { return _select ?? (_select = _partials.OfType<SelectPartial>().Single()); }
      }

      public TablePartial Table
      {
         get { return _table ?? (_table = _partials.OfType<TablePartial>().Single()); }
      }

      public IList<JoinPartial> Joins
      {
         get { return _joins ?? (_joins = _partials.OfType<JoinPartial>().ToList().AsReadOnly()); }
      }

      public WherePartial Where
      {
         get
         {
            if (_whereChecked) return _where;
            _whereChecked = true;
            return _where ?? (_where = _partials.OfType<WherePartial>().SingleOrDefault());
         }
      }

      public GroupByPartial GroupBy
      {
         get
         {
            if (_groupbyChecked) return _groupBy;
            _groupbyChecked = true;
            return _groupBy ?? (_groupBy = _partials.OfType<GroupByPartial>().SingleOrDefault());
         }
      }

      public HavingPartial Having
      {
         get
         {
            if (_havingChecked) return _having;
            _havingChecked = true;
            return _having ?? (_having = _partials.OfType<HavingPartial>().SingleOrDefault());
         }
      }

      public HavingPartial OrderBy
      {
         get
         {
            if (_orderbyChecked) return _orderBy;
            _orderbyChecked = true;
            return _orderBy ?? (_orderBy = _partials.OfType<HavingPartial>().SingleOrDefault());
         }
      }

      public QueryBuilder AsCountQuery()
      {
         if (_countQuery != null)
         {
            return _countQuery;
         }

         if (Select.Distinct && With != null)
         {
            throw new SqlParserException("Cannot build count query. DISTINCT must be moved to the CTE (must be moved inside the WITH part)");
         }

         var partials = _partials.Where(t => !(t is OrderByPartial)).ToList();

         if (!Select.Distinct)
         {
            var selectIndex = partials.IndexOf(Select);
            var countColumn = new MetaColumnPartial(new[] {"COUNT(*)"}, "c1", new ColumnMap("c1"), new ColumnMeta(true), 0);
            partials[selectIndex] = new SelectPartial(false, new SqlPartial[] {countColumn});
         }
         else
         {
            partials.Insert(0, new SqlPartial("SELECT COUNT(*) AS c1 FROM ("));
            partials.Add(new SqlPartial(") AS __inner"));
         }
         return _countQuery = new QueryBuilder(partials);
      }

      public void Format(TextWriter w, object args)
      {
         if (args == null)
         {
            w.Write(Format(null));
         }
         else
         {
            _partials.Format(w, args);
         }
      }
      public string Format(object args)
      {
         return args == null 
            ? (_formattedSql ?? (_formattedSql = _partials.Format(null))) 
            : _partials.Format(args);
      }

      public override string ToString()
      {
         return _partials.ToStringRepresentation();
      }

      public IList<SqlPartial> Partials
      {
         get { return _partials; }
      }

      public Expression<Func<IDataRecord, T>> GetBinderExpression<T>()
      {
         return GetBinderExpression(typeof(T)).CastTo<Expression<Func<IDataRecord, T>>>();
      }
      public Expression GetBinderExpression(Type entityType)
      {
         return GetBinderInfo(entityType).BinderExpression;
      }

      public QueryBuilder Map<TEntity, TMap>(Expression<Func<TEntity, TMap>> toExpression)
      {
         return Map((LambdaExpression) toExpression);
      }
      public QueryBuilder Map(LambdaExpression toExpression)
      {
         var fromType = toExpression.Parameters[0].Type;
         var toType = toExpression.Body.Type;

         return _mapCache.GetOrAdd(toType, x =>
         {

            var binderInfo = GetBinderInfo(fromType);

            var mapper = new MapVisitor(Select.Columns, binderInfo.MemberIndexMap);
            var result = mapper.Substitute(toExpression);
            var toPathMap = toType.GetMemberToFullNameMap();
            IList<SqlPartial> mappedColumns = new List<SqlPartial>();
            var index = 0;
            foreach (var colMemTuple in result.ToColumns)
            {
               var column = colMemTuple.Item1;
               var member = colMemTuple.Item2;
               var fullname = member == null ? null : toPathMap[member];
               mappedColumns.Add(new MetaColumnPartial(column, new ColumnMap(fullname), column.Meta, index++));
            }

            var partials = Partials.ToList();
            index = partials.IndexOf(Select);
            partials[index] = new SelectPartial(Select.Distinct, mappedColumns);
            var resultMap = new QueryBuilder(partials);
            resultMap._binderCache.GetOrAdd(toType, z => new BinderInfo
            {
               BinderExpression = result.ToExpression,
               MemberIndexMap = result.ToColumns.ToDictionary(t => t.Item2, t => t.Item1.Index)
            });
            return resultMap;
         });
      }

      public QueryBuilder SelectMapped(IList<string> mappedExpressions, ICollection<string> allowedAggregates = null )
      {
         allowedAggregates = allowedAggregates ?? DefaultAllowedAggregates;
         var columns = new List<MetaColumnPartial>();
         var collectedGroupColumns = new List<MetaColumnPartial>();
         var needGrouping = false;
         foreach (var e in mappedExpressions)
         {
            var name = e;
            while (name.StartsWith("(") && name.EndsWith(")"))
            {
               name = name.Substring(1, name.Length - 2);
            }

            try
            {
               string fn = null;
               if (!name.Contains("(") || !allowedAggregates.Contains(fn = name.Substring(0, name.IndexOf('('))))
               {
                  var col = Select.Columns.First(c => c.Map.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
                  columns.Add(col);
                  collectedGroupColumns.Add(col);
                  continue;
               }
              
               var args = name.Substring(fn.Length+1);
               args = args.Substring(0, args.IndexOf(')'));
               var argList = args.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
               fn = fn.Trim();
               string alias = null;
               for(var i = 0; i < argList.Length; i++)
               {
                  name = argList[i];
                  var col = Select.Columns.FirstOrDefault(c => c.Map.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
                  if (col != null)
                  {
                     argList[i] = col.Expression;
                     alias = name+"_"+fn;
                  }
               }
               if (alias == null)
               {
                  name = e;
                  throw new Exception("Error in expression: " + e);
               }
               //TODO: map fn to dialect specific aggregate??
               columns.Add(new MetaColumnPartial(new []{fn+"("+string.Join(", ",argList)+")"},alias,new ColumnMap(alias),new ColumnMeta(true),-1));
               needGrouping = true;
            }
            catch (Exception ex)
            {
               throw new XAdoSqlException("Invalid mapped column name: " + name, ex);
            }
         }
         var partials = Partials.ToList();
         partials[Partials.IndexOf(Select)] = new SelectPartial(Select.Distinct, columns.Select((c, i) => new MetaColumnPartial(c, c.Map, c.Meta, i)).Cast<SqlPartial>().ToList());
         if (needGrouping)
         {
            // because of aggregates
            var groupColumns = collectedGroupColumns.Select(c => new ColumnPartial(c.RawParts, null)).ToList();
            var current = GroupBy;
            if (current != null)
            {
               groupColumns = current.Columns.Concat(groupColumns).ToList();
               partials[Partials.IndexOf(GroupBy)] = new GroupByPartial(groupColumns, current.Expression);
            }
            else
            {
               partials.AddGroupBy(new GroupByPartial(groupColumns, null));
            }
         }
         return new QueryBuilder(partials);
      }
      public QueryBuilder SelectMapped(params string[] mappedExpressions)
      {
         return SelectMapped(mappedExpressions, null);
      }
       
      public IDictionary<string, MetaColumnPartial> GetMappedColumns()
      {
         return Select.Columns.ToDictionary(c => c.Map.FullName, c => c,StringComparer.OrdinalIgnoreCase);
      }

      public SqlGenerator.Result GetSqlFromExpression(Expression expression, IDictionary<string, object> arguments = null, ISqlDialect dialect = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         var generator = new SqlGenerator(dialect, parameterPrefix, noargs);
         return generator.Generate(expression, Select.Columns.ToDictionary(c => c.Map.FullName, c => c.Expression, StringComparer.OrdinalIgnoreCase), arguments);
      }

      public Func<IDataRecord, T> GetBinder<T>()
      {
         return GetBinder(typeof(T)).CastTo<Func<IDataRecord, T>>();
      }

      public Delegate GetBinder(Type entityType)
      {
         return GetBinderInfo(entityType).BinderDelegate;
      }

      public Func<IDataRecord,object> GetBinder(IAdoSession session)
      {
         if (_binderCache.Any())
         {
            // any will do
            return _binderCache.First().Value.ObjectBinderDelegate;
         }
         var meta = session.QueryMetaForSql(Format(new {skip = 0, take = 0, order = Select.Columns.First().Expression}));
         var type = AnonymousTypeHelper.GetOrCreateType(Select.Columns.Select(c => c.Map.FullName).ToArray(),meta.Select(m => m.AllowDBNull ? m.DataType.EnsureNullable() : m.DataType).ToArray());
         return GetBinderInfo(type).ObjectBinderDelegate;
      }

      public QueryBuilder<TEntity> ToGeneric<TEntity>()
      {
         return new QueryBuilder<TEntity>(Partials)
         {
            _binderCache = _binderCache,
            _mapCache = _mapCache,
            _countQuery = _countQuery
         };
      }


      private BinderInfo GetBinderInfo(Type entityType)
      {
         return _binderCache.GetOrAdd(entityType, type =>
         {
            var p = Expression.Parameter(typeof(IDataRecord), "row");
            IDictionary<MemberInfo, int> indices = new Dictionary<MemberInfo, int>();
            var expression = GetBinderExpression(entityType, "", p, new HashSet<string>(), indices, false);
            return new BinderInfo
            {
               BinderExpression = Expression.Lambda(expression.Convert(entityType), p),
               MemberIndexMap = indices
            };
         });
      }
      private Expression GetBinderExpression(Type refType, string path, ParameterExpression p, ISet<string> handledPathes, IDictionary<MemberInfo, int> indices, bool optional)
      {
         var ctor = refType.GetConstructor(Type.EmptyTypes);
         if (ctor == null)
         {
            throw new InvalidOperationException("Type " + refType.Name + " has no public default constructor");
         }
         var members = Select
            .Columns
            .Where(m => path.Length == 0 || m.Map.Path == path || m.Map.Path.StartsWith(path + Constants.SpecialChars.NAME_SEP_STR))
            .OrderBy(m => m.Map.Path)
            .ThenBy(m => m.Index)
            .ToArray();

         var expressions = new List<MemberBinding>();
         foreach (var m in members)
         {
            if (m.Map.Path == path)
            {
               try
               {
                  var member = refType.GetPropertyOrField(m.Map.Name);
                  var index = m.Index;
                  indices[member] = index;
                  expressions.Add(member.GetDataRecordMemberAssignmentExpression(index, p, m.Meta.NotNull));
               }
               catch (Exception ex)
               {
                  throw new Exception("Invalid member reference: " + refType.Name + Constants.SpecialChars.NAME_SEP_STR + m.Map.Name + ", map: " + m.Map + " (verify your mapping)", ex);
               }
            }
            else
            {
               if (!handledPathes.Contains(m.Map.Path))
               {
                  try
                  {
                     var refMember = refType.GetPropertyOrField(m.Map.Path.Split(Constants.SpecialChars.NAME_SEP).Last());
                     var newExpression = GetBinderExpression(MemberInfoFinder.GetMemberType(refMember), m.Map.Path, p, handledPathes, indices, m.Meta.IsOuterJoinColumn);
                     expressions.Add(Expression.Bind(refMember, newExpression));
                     handledPathes.Add(m.Map.Path);
                  }
                  catch (Exception ex)
                  {
                     throw new Exception("Invalid member reference: " + refType.Name + Constants.SpecialChars.NAME_SEP_STR + m.Map.Name + ", map: " + m.Map + " (verify your mapping)", ex);
                  }
               }
            }
         }
         var body = Expression.MemberInit(Expression.New(ctor), expressions);
         return path.Length == 0 || !optional ? (Expression)body : Expression.Condition(Expression.Call(p, IsDbNull, Expression.Constant(members[0].Index)), Expression.Constant(null).Convert(refType), body);
      }
   }

   //immutable object
   public class QueryBuilder<TEntity> : QueryBuilder
   {

      public new static QueryBuilder<TEntity> Parse(string sql, string template=null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         return QueryBuilderCache.GetOrAdd(Tuple.Create(sql, typeof(TEntity), template), x =>
         {
            var parser = new SqlSelectParser();
            var partials = parser.Parse(x.CastTo<Tuple<string, Type, string>>().Item1);
            var queryMap = new QueryBuilder(partials, x.CastTo<Tuple<string, Type, string>>().Item3);
            return queryMap.ToGeneric<TEntity>();
         }).CastTo<QueryBuilder<TEntity>>();
      }

      public QueryBuilder(IList<SqlPartial> partials, string template = null) : base(partials,template)
      {
      }

      public Expression<Func<IDataRecord, TEntity>> GetBinderExpression()
      {
         return GetBinderExpression<TEntity>();
      }

      public QueryBuilder<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression)
      {
         return Map<TEntity, TMap>(toExpression).ToGeneric<TMap>();
      }

      public Func<IDataRecord, TEntity> GetBinder()
      {
         return GetBinder<TEntity>();
      }

      public new SqlGenerator.Result GetSqlFromExpression(Expression<Func<TEntity,bool>> expression, IDictionary<string, object> arguments = null, ISqlDialect dialect = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return base.GetSqlFromExpression(expression, arguments, dialect, parameterPrefix, noargs);
      }
      public new SqlGenerator.Result GetSqlFromExpression(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null, ISqlDialect dialect = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return base.GetSqlFromExpression(expression, arguments, dialect, parameterPrefix, noargs);
      }

   }

}
