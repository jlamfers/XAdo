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
using XAdo.Core.Cache;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Mapper;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.Core
{
   // immutable object

   public partial class QueryBuilder : IQueryBuilder
   {
      private readonly ICache<Type, BinderInfo>
         _binderCache = new SmallCache<Type, BinderInfo>();

      private readonly LRUCache<object, QueryBuilder>
         _mapCache = new LRUCache<object, QueryBuilder>("LRUCache.SubSet.Size", 25);

      private readonly LRUCache<string, SqlGenerator.Result>
         _compiledSqlCache = new LRUCache<string, SqlGenerator.Result>("LRUCache.CompiledSql.Size", 500, StringComparer.OrdinalIgnoreCase);

      private string
         _formattedSql;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IDictionary<string,ColumnPartial> 
         _mappedColumns;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IDictionary<string, string> 
         _mappedExpressions;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IList<TablePartial>
         _tables;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly MethodInfo
         IsDbNull = MemberInfoFinder.GetMethodInfo<IDataRecord>(r => r.IsDBNull(0));

      private QueryBuilder
         _countQuery;



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
            var countColumn = new ColumnPartial(new[] { "COUNT(*)" }, "c1",null,new ColumnMap("c1"),0 );
            partials[selectIndex] = new SelectPartial(false, new []{ countColumn });
         }
         else
         {
            partials.Insert(0, new SqlPartial("SELECT COUNT(*) AS c1 FROM ("));
            partials.Add(new SqlPartial(") AS __inner"));
         }
         return _countQuery = CreateMap(partials);
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
         return Map((LambdaExpression)toExpression);
      }
      public QueryBuilder Map(LambdaExpression toExpression)
      {
         var fromType = toExpression.Parameters[0].Type;
         var toType = toExpression.Body.Type;

         return _mapCache.GetOrAdd(toExpression.GetKey(), x =>
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
            var resultMap = CreateMap(partials);
            resultMap._binderCache.GetOrAdd(toType, z => new BinderInfo
            {
               BinderExpression = result.ToExpression,
               MemberIndexMap = result.ToColumns.ToDictionary(t => t.Item2, t => t.Item1.Index)
            });
            return resultMap;
         });
      }
      public QueryBuilder Map(string selectExpression, Type mappedType)
      {
         return _mapCache.GetOrAdd(selectExpression, x =>
         {
            mappedType = mappedType ?? GetBinderType(null);

            var columnTuples = _urlParser.SplitColumns(selectExpression);
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
                  var expression = _urlParser.Parse(col.Item1, mappedType, typeof (object));
                  var result = BuildSqlByExpression(expression, null);
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
                      Select.Columns.ToDictionary(c => c.Map.FullName, c => c.Expression, StringComparer.OrdinalIgnoreCase)
                         .AsReadOnly());
         }
      }

      public IList<TablePartial> Tables
      {
         get
         {
            if (_tables != null) return _tables;
            var tables = new[] {Table}.Concat(Joins==null ? Enumerable.Empty<TablePartial>() : Joins.Select(j => j.RighTable)).ToList();
            return _tables = tables.AsReadOnly();

         }
      } 

      public SqlGenerator.Result BuildSqlByExpression(Expression expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         var result = _compiledSqlCache.GetOrAdd(expression.GetKey(), x =>
         {
            var generator = new SqlGenerator(Dialect, parameterPrefix, noargs);
            return generator.Generate(expression, MappedExpressions, null);
         });
         if (arguments != null)
         {
            arguments.AddRange(result.Arguments);
            return new SqlGenerator.Result(result.Sql, arguments);
         }
         return new SqlGenerator.Result(result.Sql, result.Arguments.ToDictionary(x => x.Key, x => x.Value));
      }
      public SqlGenerator.Result BuildSqlPredicate(string expression, Type mappedType, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         var result = _compiledSqlCache.GetOrAdd(expression, x =>
         {
            var expr = _urlParser.Parse(expression, mappedType ?? GetBinderType(null),typeof (bool));
            var generator = new SqlGenerator(Dialect, parameterPrefix, noargs);
            return generator.Generate(expr, MappedExpressions, null);
         }
            );
         if (arguments != null)
         {
            arguments.AddRange(result.Arguments);
            return new SqlGenerator.Result(result.Sql,arguments);
         }
         return new SqlGenerator.Result(result.Sql, result.Arguments.ToDictionary(x => x.Key, x => x.Value));
      }
      public string BuildSqlOrderBy(string orderExpression, Type mappedType)
      {
         mappedType = mappedType ?? GetBinderType(null);
         var columnTuples = _urlParser.SplitColumns(orderExpression);
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
               var expression = _urlParser.Parse(columnName, mappedType, typeof(object));
               sb.Append(BuildSqlByExpression(expression));
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
            sb.Append(BuildSqlByExpression(item1).Sql);
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
         return GetBinder(typeof(T)).CastTo<Func<IDataRecord, T>>();
      }
      public Delegate GetBinder(Type entityType)
      {
         return GetBinderInfo(entityType).BinderDelegate;
      }
      public Func<IDataRecord, object> GetBinder(IAdoSession session)
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
            throw new InvalidOperationException("Need session here to retrieve meta data. Now session is not allowed null.");
         }

         var type = AnonymousTypeHelper.GetOrCreateType(Select.Columns.Select(c => c.Map.FullName).ToArray(), Select.Columns.Select(c => c.Meta.IsNotNull ? c.Meta.Type : c.Meta.Type.EnsureNullable()).ToArray());
         return GetBinderInfo(type).ObjectBinderDelegate;

      }
      public Type GetBinderType(IAdoSession session)
      {
         if (_binderCache.Any())
         {
            // any type will do
            return _binderCache.First().Value.EntityType;
         }
         if (session == null)
         {
            throw new InvalidOperationException("Cannot determine type yet. Need a specified type, or an ado-session (for retrieving meta data), first");
         }
         var meta = session.QueryMetaForSql(Format(new { skip = 0, take = 0, order = Select.Columns.First().Expression }));
         var type = AnonymousTypeHelper.GetOrCreateType(Select.Columns.Select(c => c.Map.FullName).ToArray(), meta.Select(m => m.AllowDBNull ? m.DataType.EnsureNullable() : m.DataType).ToArray());
         return GetBinderInfo(type).BinderExpression.Parameters[0].Type;
      }

      public QueryBuilder<TEntity> ToGeneric<TEntity>()
      {
         return new QueryBuilder<TEntity>(this);
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
            .Where(m => path.Length == 0 || m.Map.Path == path || m.Map.Path.StartsWith(path + Constants.Syntax.Chars.NAME_SEP_STR))
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
                  throw new Exception("Invalid member reference: " + refType.Name + Constants.Syntax.Chars.NAME_SEP_STR + m.Map.Name + ", map: " + m.Map + " (verify your mapping)", ex);
               }
            }
            else
            {
               if (!handledPathes.Contains(m.Map.Path))
               {
                  try
                  {
                     var refMember = refType.GetPropertyOrField(m.Map.Path.Split(Constants.Syntax.Chars.NAME_SEP).Last());
                     var newExpression = GetBinderExpression(MemberInfoFinder.GetMemberType(refMember), m.Map.Path, p, handledPathes, indices, m.Meta.IsOuterJoinColumn);
                     expressions.Add(Expression.Bind(refMember, newExpression));
                     handledPathes.Add(m.Map.Path);
                  }
                  catch (Exception ex)
                  {
                     throw new Exception("Invalid member reference: " + refType.Name + Constants.Syntax.Chars.NAME_SEP_STR + m.Map.Name + ", map: " + m.Map + " (verify your mapping)", ex);
                  }
               }
            }
         }
         var body = Expression.MemberInit(Expression.New(ctor), expressions);
         return path.Length == 0 || !optional ? (Expression)body : Expression.Condition(Expression.Call(p, IsDbNull, Expression.Constant(members[0].Index)), Expression.Constant(null).Convert(refType), body);
      }


      private bool _adoMetaDataBound = false;
      private void EnsureAdoMetaDataBound(IAdoSession session)
      {
         if (_adoMetaDataBound)
         {
            return;
         }
         var metaList = Tables.ToDictionary(t => t, t => session.QueryMetaForTable(t.Expression).ToDictionary(mt => mt.ColumnName, mt => mt, StringComparer.OrdinalIgnoreCase));
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
            var meta = session.QueryMetaForSql(Format(new { skip = 0, take = 0, order = Select.Columns.First().Expression }));
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


   }

}