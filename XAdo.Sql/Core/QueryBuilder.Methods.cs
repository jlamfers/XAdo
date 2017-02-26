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
using XAdo.Sql.Core.Common;
using XAdo.Sql.Core.Linq;
using XAdo.Sql.Core.Mapper;
using XAdo.Sql.Core.Parser;
using XAdo.Sql.Core.Parser.Partials;
using XAdo.Sql.Dialects;
using XAdo.Sql.Linq;

namespace XAdo.Sql.Core
{
   // immutable object

   public partial class QueryBuilder : IQueryBuilder
   {
      private readonly ICache<Type, BinderInfo>
         _binderCache = new SmallCache<Type, BinderInfo>();

      private readonly LRUCache<object, QueryBuilder>
         _mapCache = new LRUCache<object, QueryBuilder>("LRUCache.SubSet.Size", 25);

      private string
         _formattedSql;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IDictionary<string, MetaColumnPartial> 
         _mappedColumns;

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
            var countColumn = new MetaColumnPartial(new[] { "COUNT(*)" }, "c1", new ColumnMap("c1"), new ColumnMeta(true), 0);
            partials[selectIndex] = new SelectPartial(false, new SqlPartial[] { countColumn });
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
            var columns = new List<MetaColumnPartial>();
            var nonAggregateColumns = new List<MetaColumnPartial>();
            var needGrouping = false;
            foreach (var col in columnTuples)
            {
               MetaColumnPartial column;
               if (MappedColumns.TryGetValue(col.Item1, out column))
               {
                  var alias = !string.IsNullOrEmpty(col.Item2) ? col.Item2 : column.Alias;
                  var fullname = !string.IsNullOrEmpty(col.Item2) ? col.Item2 : column.Map.FullName;
                  column = new MetaColumnPartial(column.RawParts, alias, new ColumnMap(fullname), column.Meta,
                     columns.Count);
                  columns.Add(column);
                  nonAggregateColumns.Add(column);
               }
               else
               {
                  var expression = _urlParser.Parse(col.Item1, mappedType, typeof (object));
                  var result = BuildSqlFromExpression(expression, null);
                  var lparenIndex = result.Sql.IndexOf('(');
                  if (lparenIndex != -1 && _dialect.GetAggregates().Contains(result.Sql.Substring(0, lparenIndex)))
                  {
                     needGrouping = true;
                  }
                  var alias = !string.IsNullOrEmpty(col.Item2) ? col.Item2 : "xado_expr_" + columns.Count;
                  columns.Add(new MetaColumnPartial(new[] {col.Item1}, alias, new ColumnMap(alias), new ColumnMeta(true),
                     columns.Count));
               }
            }
            var partials = Partials.ToList();
            partials[Partials.IndexOf(Select)] = new SelectPartial(Select.Distinct, columns.Cast<SqlPartial>().ToList());
            if (needGrouping)
            {
               // because of aggregates
               var groupColumns = nonAggregateColumns.Select(c => new ColumnPartial(c.RawParts, null)).ToList();
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

      public IDictionary<string, MetaColumnPartial> MappedColumns
      {
         get
         {
            return _mappedColumns ??
                   (_mappedColumns =
                      Select.Columns.ToDictionary(c => c.Map.FullName, c => c, StringComparer.OrdinalIgnoreCase)
                         .AsReadOnly());
         }
      }

      public SqlGenerator.Result BuildSqlFromExpression(Expression expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         var generator = new SqlGenerator(_dialect, parameterPrefix, noargs);
         return generator.Generate(expression, Select.Columns.ToDictionary(c => c.Map.FullName, c => c.Expression, StringComparer.OrdinalIgnoreCase), arguments);
      }
      public SqlGenerator.Result GetSqlPredicate(string expression, Type mappedType, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return BuildSqlFromExpression(_urlParser.Parse(expression, mappedType ?? GetBinderType(null),typeof(bool)), arguments,parameterPrefix, noargs);
      }
      public string GetSqlOrderBy(string orderExpression, Type mappedType)
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
            MetaColumnPartial column;
            if (MappedColumns.TryGetValue(columnName, out column))
            {
               sb.Append(column.Expression);
            }
            else
            {
               var expression = _urlParser.Parse(columnName, mappedType, typeof(object));
               sb.Append(BuildSqlFromExpression(expression));
            }
            if (desc)
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
         //TODO: find metadata by tables
         if (_binderCache.Any())
         {
            // any will do
            return _binderCache.First().Value.ObjectBinderDelegate;
         }
         var meta = session.QueryMetaForSql(Format(new { skip = 0, take = 0, order = Select.Columns.First().Expression }));
         var type = AnonymousTypeHelper.GetOrCreateType(Select.Columns.Select(c => c.Map.FullName).ToArray(), meta.Select(m => m.AllowDBNull ? m.DataType.EnsureNullable() : m.DataType).ToArray());
         return GetBinderInfo(type).ObjectBinderDelegate;
      }
      public Type GetBinderType(IAdoSession session)
      {
         if (_binderCache.Any())
         {
            // any type will do
            return _binderCache.First().Value.BinderExpression.Parameters[0].Type;
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

}