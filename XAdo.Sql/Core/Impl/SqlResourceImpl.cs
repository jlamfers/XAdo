using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XAdo.Core;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Expressions;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Impl
{
   // immutable object
   public class SqlResourceImpl : ISqlResource
   {

      #region Types
      private class MapVisitor : ExpressionVisitor
      {
         public class VisitResult
         {
            public List<Tuple<ColumnPartial, MemberInfo>> ToColumns { get; set; }
            public LambdaExpression ToExpression { get; set; }
         }

         private readonly IList<ColumnPartial> _fromColumns;
         private readonly IDictionary<MemberInfo, int> _fromIndices;
         private ParameterExpression _parameter;
         private List<Tuple<ColumnPartial, MemberInfo>> _toColumns;

         public MapVisitor(IList<ColumnPartial> fromColumns, IDictionary<MemberInfo, int> fromIndices)
         {
            _fromColumns = fromColumns;
            _fromIndices = fromIndices;
         }

         public VisitResult Substitute(LambdaExpression fromExpression)
         {
            _toColumns = new List<Tuple<ColumnPartial, MemberInfo>>();
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
               args.Add(node.Members[i].GetDataRecordRecordGetterExpression(newIndex, _parameter, column.Meta.IsNotNull));
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
               return node.Member.GetDataRecordMemberAssignmentExpression(newIndex, _parameter, column.Meta.IsNotNull);
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
               return node.Member.GetDataRecordRecordGetterExpression(newIndex, _parameter, column.Meta.IsNotNull);
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
            get
            {
               return _binderDelegate ?? (_binderDelegate = typeof(Compiler<>)
                  .MakeGenericType(BinderExpression.Body.Type)
                  .CreateInstance()
                  .CastTo<ICompiler>()
                  .Compile(BinderExpression));
            }
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

         public Type EntityType { get { return BinderExpression.Body.Type; } }
      }

      #endregion

      private IFilterParser 
         _filterParser;

      private ISqlPredicateGenerator 
         _sqlPredicateGenerator;

      private ITemplateFormatter
         _templateFormatter;

      private ISqlBuilder 
         _sqlBuilder;

      private readonly SmallCache<Type, BinderInfo>
         _binderCache = new SmallCache<Type, BinderInfo>();

      private readonly LRUCache<object, ISqlResource>
         _subResourcesCache = new LRUCache<object, ISqlResource>("LRUCache.SubResource.Size", 25);

      private readonly LRUCache<string, SqlGeneratorResult>
         _compiledSqlCache = new LRUCache<string, SqlGeneratorResult>("LRUCache.CompiledSql.Size", 500,
            StringComparer.OrdinalIgnoreCase);

      private string
         _sqlSelectTemplate,
         _sqlCountTemplate;


      #region Hidden fields
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IList<SqlPartial> _partials;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private WithPartial _with;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _withChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private SelectPartial _select;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private TablePartial _table;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IList<JoinPartial> _joins;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private WherePartial _where;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _whereChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private GroupByPartial _groupBy;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _groupbyChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private HavingPartial _having;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _havingChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private HavingPartial _orderBy;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _orderbyChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IDictionary<string, ColumnPartial> _mappedColumns;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IDictionary<string, string> _mappedExpressions;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IList<TablePartial> _tables;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly MethodInfo IsDbNull = MemberInfoFinder.GetMethodInfo<IDataRecord>(r => r.IsDBNull(0));
      #endregion

      protected SqlResourceImpl(SqlResourceImpl other)
      {
         _partials = other._partials;
         Dialect = other.Dialect;
         _binderCache = other._binderCache;
         _subResourcesCache = other._subResourcesCache;
         _filterParser = other._filterParser;
         _sqlPredicateGenerator = other._sqlPredicateGenerator;
         _templateFormatter = other._templateFormatter;
         _sqlBuilder = other._sqlBuilder;

         _sqlSelectTemplate = other._sqlSelectTemplate;
         _sqlCountTemplate = other._sqlCountTemplate;
      }

      protected SqlResourceImpl()
      {

      }

      public SqlResourceImpl(IList<SqlPartial> partials, ISqlDialect dialect, IFilterParser filterParser, ISqlPredicateGenerator sqlPredicateGenerator, ITemplateFormatter templateFormatter, ISqlBuilder sqlBuilder)
      {
         if (partials == null) throw new ArgumentNullException("partials");
         if (dialect == null) throw new ArgumentNullException("dialect");
         if (filterParser == null) throw new ArgumentNullException("filterParser");
         if (sqlPredicateGenerator == null) throw new ArgumentNullException("sqlPredicateGenerator");
         if (templateFormatter == null) throw new ArgumentNullException("templateFormatter");
         if (sqlBuilder == null) throw new ArgumentNullException("sqlBuilder");

         Dialect = dialect;
         _filterParser = filterParser;
         _sqlPredicateGenerator = sqlPredicateGenerator;
         _partials = partials.EnsureLinked().MergeTemplate(dialect.SelectTemplate).AsReadOnly();
         _templateFormatter = templateFormatter;
         _sqlBuilder = sqlBuilder;
      }

      public ISqlDialect Dialect { get; private set; }

      public ISqlResource CreateMap(IList<SqlPartial> partials)
      {
         var mapped = new SqlResourceImpl
         {
            _partials = partials.EnsureLinked().ToList().AsReadOnly(),
            Dialect = Dialect,
            _filterParser = _filterParser,
            _sqlPredicateGenerator = _sqlPredicateGenerator,
            _templateFormatter = _templateFormatter,
            _sqlBuilder = _sqlBuilder
         };
         return mapped;
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
         get { return _table ?? (_table = _partials.OfType<FromTablePartial>().Single().Table); }
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

      #region Methods

      public string BuildSqlSelect(object args)
      {
         return _templateFormatter.Format(SqlSelectTemplate, args);
      }
      public string BuildSqlCount(object args)
      {
         return _templateFormatter.Format(SqlCountTemplate, args);
      }

      public string SqlSelectTemplate
      {
         get { return _sqlSelectTemplate ?? (_sqlSelectTemplate = _sqlBuilder.BuildSelect(this)); }
      }
      public string SqlCountTemplate
      {
         get { return _sqlCountTemplate ?? (_sqlCountTemplate = _sqlBuilder.BuildCount(this)); }
      }

      public override string ToString()
      {
         return SqlSelectTemplate;
      }

      public IList<SqlPartial> Partials
      {
         get { return _partials; }
      }

      public Expression<Func<IDataRecord, T>> GetBinderExpression<T>()
      {
         return GetBinderExpression(typeof (T)).CastTo<Expression<Func<IDataRecord, T>>>();
      }

      public Expression GetBinderExpression(Type entityType)
      {
         return GetBinderInfo(entityType).BinderExpression;
      }

      public ISqlResource Map(LambdaExpression toExpression)
      {
         var fromType = toExpression.Parameters[0].Type;
         var toType = toExpression.Body.Type;

         return _subResourcesCache.GetOrAdd(toExpression.GetKey(), x =>
         {

            var binderInfo = GetBinderInfo(fromType);

            var mapper = new MapVisitor(Select.Columns.Clone(), binderInfo.MemberIndexMap);
            var result = mapper.Substitute(toExpression);
            var toPathMap = toType.GetMemberToFullNameMap();
            IList<ColumnPartial> mappedColumns = new List<ColumnPartial>();
            var index = 0;
            foreach (var colMemTuple in result.ToColumns)
            {
               var column = colMemTuple.Item1;
               var member = colMemTuple.Item2;
               var fullname = member == null ? null : toPathMap[member];
               column.SetMap(new ColumnMap(fullname));
               column.SetIndex(index++);
               mappedColumns.Add(column);
            }

            var partials = Partials.ToList();
            index = partials.IndexOf(Select);
            partials[index] = new SelectPartial(Select.Distinct, mappedColumns);
            var resultMap = (SqlResourceImpl) CreateMap(partials);
            resultMap._binderCache.GetOrAdd(toType, z => new BinderInfo
            {
               BinderExpression = result.ToExpression,
               MemberIndexMap = result.ToColumns.ToDictionary(t => t.Item2, t => t.Item1.Index)
            });
            return resultMap;
         });
      }

      public ISqlResource Map(string selectExpression, Type entityType)
      {
         return _subResourcesCache.GetOrAdd(selectExpression, x =>
         {
            entityType = entityType ?? GetEntityType(null);

            var columnTuples = _filterParser.SplitColumns(selectExpression);
            var columns = new List<ColumnPartial>();
            var nonAggregateColumns = new List<ColumnPartial>();
            var needGrouping = false;
            foreach (var col in columnTuples)
            {
               ColumnPartial column;
               if (MappedColumns.TryGetValue(col.Item1, out column))
               {
                  column = column.Clone();
                  var alias = !string.IsNullOrEmpty(col.Item2) ? col.Item2 : column.Alias;
                  var fullname = !string.IsNullOrEmpty(col.Item2) ? col.Item2 : column.Map.FullName;
                  column.SetAlias(alias);
                  column.SetMap(new ColumnMap(fullname));
                  column.SetIndex(columns.Count);
                  columns.Add(column);
                  nonAggregateColumns.Add(column);
               }
               else
               {
                  var expression = _filterParser.Parse(col.Item1, entityType, typeof (object));
                  var result = BuildSql(expression, null);
                  var lparenIndex = result.Sql.IndexOf('(');
                  if (lparenIndex != -1 && Dialect.GetAggregates().Contains(result.Sql.Substring(0, lparenIndex)))
                  {
                     needGrouping = true;
                  }
                  var alias = !string.IsNullOrEmpty(col.Item2) ? col.Item2 : "xado_expr_" + columns.Count;
                  // MUST be read only
                  columns.Add(new ColumnPartial(new[] {col.Item1}, alias, null, new ColumnMap(alias), columns.Count));
               }
            }
            var partials = Partials.ToList();
            partials[Partials.IndexOf(Select)] = new SelectPartial(Select.Distinct, columns.ToList());
            if (needGrouping)
            {
               // because of aggregates
               var groupColumns = nonAggregateColumns;
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
            return CreateMap(partials);
         });
      }

      public IDictionary<string, ColumnPartial> MappedColumns
      {
         get
         {
            return _mappedColumns ??
                   (_mappedColumns =
                      Select.Columns.ToDictionary(c => c.Map.FullName, c => c, StringComparer.OrdinalIgnoreCase)
                         .AsReadOnly());
         }
      }

      public IDictionary<string, string> MappedExpressions
      {
         get
         {
            return _mappedExpressions ??
                   (_mappedExpressions =
                      Select.Columns.ToDictionary(c => c.Map.FullName, c => c.Expression,
                         StringComparer.OrdinalIgnoreCase)
                         .AsReadOnly());
         }
      }

      public IList<TablePartial> Tables
      {
         get
         {
            if (_tables != null) return _tables;
            var tables =
               new[] {Table}.Concat(Joins == null ? Enumerable.Empty<TablePartial>() : Joins.Select(j => j.RighTable))
                  .ToList();
            return _tables = tables.AsReadOnly();

         }
      }

      public SqlGeneratorResult BuildSql(Expression expression, IDictionary<string, object> arguments = null)
      {
         var result = _compiledSqlCache.GetOrAdd(expression.GetKey(),
            x => _sqlPredicateGenerator.Generate(expression, MappedExpressions, null));
         if (arguments != null)
         {
            arguments.AddRange(result.Arguments);
            return new SqlGeneratorResult(result.Sql, arguments);
         }
         return new SqlGeneratorResult(result.Sql, result.Arguments.ToDictionary(x => x.Key, x => x.Value));
      }

      public SqlGeneratorResult BuildSqlPredicate(string expression, Type mappedType,
         IDictionary<string, object> arguments = null)
      {
         var result = _compiledSqlCache.GetOrAdd(expression, x =>
         {
            var expr = _filterParser.Parse(expression, mappedType ?? GetEntityType(null), typeof (bool));
            return _sqlPredicateGenerator.Generate(expr, MappedExpressions, null);
         }
            );
         if (arguments != null)
         {
            arguments.AddRange(result.Arguments);
            return new SqlGeneratorResult(result.Sql, arguments);
         }
         return new SqlGeneratorResult(result.Sql, result.Arguments.ToDictionary(x => x.Key, x => x.Value));
      }

      public string BuildSqlOrderBy(string orderExpression, Type mappedType)
      {
         mappedType = mappedType ?? GetEntityType(null);
         var columnTuples = _filterParser.SplitColumns(orderExpression);
         var sb = new StringBuilder();
         var comma = "";
         foreach (var item1 in columnTuples.Select(t => t.Item1))
         {
            sb.Append(comma);
            var columnName = item1;
            var desc = columnName.StartsWith("-");
            if (desc)
            {
               columnName = columnName.Substring(1);
            }
            ColumnPartial column;
            if (MappedColumns.TryGetValue(columnName, out column))
            {
               sb.Append(column.Expression);
            }
            else
            {
               var expression = _filterParser.Parse(columnName, mappedType, typeof (object));
               sb.Append(BuildSql(expression));
            }
            if (desc)
            {
               sb.Append(" DESC");
            }
            comma = ", ";
         }
         return sb.ToString();
      }

      public string BuildSqlOrderBy(bool descending, params Expression[] columns)
      {
         var sb = new StringBuilder();
         var comma = "";
         foreach (var item1 in columns)
         {
            sb.Append(comma);
            sb.Append(BuildSql(item1).Sql);
            if (descending)
            {
               sb.Append(" DESC");
            }
            comma = ", ";
         }
         return sb.ToString();
      }

      public Func<IDataRecord, T> GetBinder<T>()
      {
         return GetBinder(typeof (T)).CastTo<Func<IDataRecord, T>>();
      }

      public Delegate GetBinder(Type entityType)
      {
         return GetBinderInfo(entityType).BinderDelegate;
      }

      public Func<IDataRecord, object> GetBinder(IXAdoDbSession session)
      {
         if (session != null)
         {
            EnsureAdoMetaDataBound(session);
         }

         if (_binderCache.Any())
         {
            // any will do
            return _binderCache.First().Value.ObjectBinderDelegate;
         }

         if (session == null)
         {
            throw new QuobException(
               "Need session here to retrieve meta data. Now session is not allowed null.");
         }

         var type = AnonymousTypeHelper.GetOrCreateType(Select.Columns.Select(c => c.Map.FullName).ToArray(),
            Select.Columns.Select(c => c.Meta.IsNotNull ? c.Meta.Type : c.Meta.Type.EnsureNullable()).ToArray());
         return GetBinderInfo(type).ObjectBinderDelegate;

      }

      public Type GetEntityType(IXAdoDbSession session)
      {
         if (_binderCache.Any())
         {
            // any type will do
            return _binderCache.First().Value.EntityType;
         }
         if (session == null)
         {
            return null;
         }
         EnsureAdoMetaDataBound(session);
         var type = AnonymousTypeHelper.GetOrCreateType(Select.Columns.Select(c => c.Map.FullName).ToArray(),
            Select.Columns.Select(c => c.Meta.Type).ToArray());
         return GetBinderInfo(type).EntityType;
      }

      public ISqlResource<TEntity> ToGeneric<TEntity>()
      {
         return new SqlResource<TEntity>(this);
      }

      private BinderInfo GetBinderInfo(Type entityType)
      {
         return _binderCache.GetOrAdd(entityType, type =>
         {
            var p = Expression.Parameter(typeof (IDataRecord), "row");
            IDictionary<MemberInfo, int> indices = new Dictionary<MemberInfo, int>();
            var expression = GetBinderExpression(entityType, "", p, new HashSet<string>(), indices, false);
            return new BinderInfo
            {
               BinderExpression = Expression.Lambda(expression.Convert(entityType), p),
               MemberIndexMap = indices
            };
         });
      }

      private Expression GetBinderExpression(Type refType, string path, ParameterExpression p,
         ISet<string> handledPathes, IDictionary<MemberInfo, int> indices, bool optional)
      {
         var ctor = refType.GetConstructor(Type.EmptyTypes);
         if (ctor == null)
         {
            throw new QuobException("Type " + refType.Name + " has no public default constructor");
         }
         var members = Select
            .Columns
            .Where(
               m =>
                  path.Length == 0 || m.Map.Path == path ||
                  m.Map.Path.StartsWith(path + Constants.Syntax.Chars.NAME_SEP_STR))
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
                  expressions.Add(member.GetDataRecordMemberAssignmentExpression(index, p, m.Meta.IsNotNull));
               }
               catch (Exception ex)
               {
                  throw new QuobException(
                     "Invalid member reference: " + refType.Name + Constants.Syntax.Chars.NAME_SEP_STR + m.Map.Name +
                     ", map: " + m.Map + " (verify your mapping)", ex);
               }
            }
            else
            {
               if (!handledPathes.Contains(m.Map.Path))
               {
                  try
                  {
                     var refMember = refType.GetPropertyOrField(m.Map.Path.Split(Constants.Syntax.Chars.NAME_SEP).Last());
                     var newExpression = GetBinderExpression(MemberInfoFinder.GetMemberType(refMember), m.Map.Path, p,
                        handledPathes, indices, m.Meta.IsOuterJoinColumn);
                     expressions.Add(Expression.Bind(refMember, newExpression));
                     handledPathes.Add(m.Map.Path);
                  }
                  catch (Exception ex)
                  {
                     throw new QuobException(
                        "Invalid member reference: " + refType.Name + Constants.Syntax.Chars.NAME_SEP_STR + m.Map.Name +
                        ", map: " + m.Map + " (verify your mapping)", ex);
                  }
               }
            }
         }
         var body = Expression.MemberInit(Expression.New(ctor), expressions);
         return path.Length == 0 || !optional
            ? (Expression) body
            : Expression.Condition(Expression.Call(p, IsDbNull, Expression.Constant(members[0].Index)),
               Expression.Constant(null).Convert(refType), body);
      }

      private bool _adoMetaDataBound;

      private void EnsureAdoMetaDataBound(IXAdoDbSession session)
      {
         if (_adoMetaDataBound)
         {
            return;
         }
         var metaList = Tables.ToDictionary(t => t,
            t =>
               session.QueryMetaForTable(t.Expression)
                  .ToDictionary(mt => mt.ColumnName, mt => mt, StringComparer.OrdinalIgnoreCase));
         var anyNullTable = false;
         for (var i = 0; i < Select.Columns.Count; i++)
         {
            var c = Select.Columns[i];
            if (c.Table == null)
            {
               anyNullTable = true;
               continue;
            }
            c.Meta.InitializeByAdoMeta(metaList[c.Table][c.ColumnName]);
         }

         if (anyNullTable)
         {
            var meta =
               session.QueryMetaForSql(BuildSqlSelect(new {skip = 0, take = 0, order = Select.Columns.First().Expression}));
            for (var i = 0; i < Select.Columns.Count; i++)
            {
               var c = Select.Columns[i];
               if (c.Table != null)
               {
                  continue;
               }
               c.Meta.InitializeByAdoMeta(meta[i]);
            }
            _adoMetaDataBound = true;
            return;
         }


         _adoMetaDataBound = true;
      }

      #endregion

   }
}






