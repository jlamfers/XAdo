using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.DbSchema.Attributes;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlObjects.Core;

namespace XAdo.SqlObjects.SqlExpression.Visitors
{
   public class BinderExpressionVisitor : ExpressionVisitor
   {
      private readonly ISqlFormatter _formatter;

      public static class NullableGetters
      {
         public static object GetNValue(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? null : reader.GetValue(index);
         }
         public static string GetNString(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? null : reader.GetString(index);
         }
         public static Byte? GetNByte(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Byte?)null : reader.GetByte(index);
         }

         public static Boolean? GetNBoolean(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Boolean?)null : reader.GetBoolean(index);
         }

         public static Char? GetNChar(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Char?)null : reader.GetChar(index);
         }

         public static Decimal? GetNDecimal(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Decimal?)null : reader.GetDecimal(index);
         }

         public static Double? GetNDouble(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Double?)null : reader.GetDouble(index);
         }

         public static Single? GetNFloat(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Single?)null : reader.GetFloat(index);
         }

         public static Guid? GetNGuid(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Guid?)null : reader.GetGuid(index);
         }

         public static Int16? GetNInt16(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Int16?)null : reader.GetInt16(index);
         }

         public static Int32? GetNInt32(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Int32?)null : reader.GetInt32(index);
         }

         public static Int64? GetNInt64(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Int64?)null : reader.GetInt64(index);
         }

         public static DateTime? GetNDateTime(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (DateTime?)null : reader.GetDateTime(index);
         }
      }

      public class MappedMemberInfo
      {
         public MappedMemberInfo(MemberInfo member, Expression expression, string sql, string @alias)
         {
            Alias = alias;
            Sql = sql;
            Expression = expression;
            Member = member;
         }

         public MemberInfo Member { get; private set; }
         public Expression Expression { get; private set; }
         public string Sql { get; private set; }
         public string Alias { get; private set; }
      }

      public class ColumnInfo
      {
         private int _hashcode;
         private MemberInfo _mappedMember;

         public ColumnInfo(string sql, int index, MemberInfo mappedMember)
         {
            Sql = sql;
            Index = index;
            _mappedMember = mappedMember;
            InitHashCode();
         }

         private void InitHashCode()
         {
            unchecked
            {
               _hashcode = Sql.GetHashCode() * 541;
               if (_mappedMember != null)
               {
                  _hashcode += _mappedMember.GetHashCode();
               }
            }
         }

         public string Sql { get; private set; }
         public int Index { get; private set; }
         public string Alias
         {
            get { return Aliases.Column(Index); }
         }

         public MemberInfo MappedMember
         {
            get { return _mappedMember; }
            internal set
            {
               _mappedMember = value;
               InitHashCode();
            }
         }

         public override int GetHashCode()
         {
            return _hashcode;
         }
         public override bool Equals(object obj)
         {
            var other = obj as ColumnInfo;
            return other != null && other.Sql == Sql && other.MappedMember == MappedMember;
         }
      }

      public class CompileResult<T> 
      {
         public List<ColumnInfo> Columns { get; set; }
         public List<QueryChunks.Join> Joins { get; set; }
         public Dictionary<MemberInfo, MappedMemberInfo> MemberMap { get; set; }
         public ParameterExpression OrigParameter { get; set; }
         public Expression<Func<IDataRecord, T>> BinderExpression { get; set; }
      }

      private ParameterExpression
         _parameter;

      private Dictionary<ColumnInfo, ColumnInfo>
         _columns;

      private List<DbSchemaDescriptor.JoinPath>
         _joins;

      private Dictionary<MemberInfo, MappedMemberInfo>
         _memberMap;

      private ParameterExpression
         _origParameter;

      public BinderExpressionVisitor(ISqlFormatter formatter)
      {
         _formatter = formatter;
      }

      public CompileResult<T> Compile<T>(LambdaExpression expression,List<DbSchemaDescriptor.JoinPath> joins)
      {
         _parameter = Expression.Parameter(typeof(IDataRecord), "rdr");
         _columns = new Dictionary<ColumnInfo, ColumnInfo>();
         _joins = joins ?? new List<DbSchemaDescriptor.JoinPath>();
         _memberMap = new Dictionary<MemberInfo, MappedMemberInfo>();
         _origParameter = expression.Parameters.Single();
         var body = Visit(expression.Body);
         var binderExpression = Expression.Lambda<Func<IDataRecord, T>>(body, _parameter);
         return new CompileResult<T>
         {
            BinderExpression = binderExpression,
            Joins = _joins.SelectMany(j => j.Joins).Select(j =>  new QueryChunks.Join(j.JoinInfo.Format(_formatter,j.LeftTableAlias,j.RightTableAlias),j.JoinType.ToJoinTypeString())).ToList(),
            Columns = _columns.Keys.OrderBy(c => c.Index).ToList(),
            MemberMap = _memberMap,
            OrigParameter = _origParameter
         };
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
         return node == _origParameter ? _parameter : node;
      }

      protected override Expression VisitNew(NewExpression node)
      {
         var members = node.Members;
         var arguments = new List<Expression>();
         if (members != null && members.Count > 0)
         {
            for (var i = 0; i < members.Count; i++)
            {
               if (!node.Arguments[i].Type.IsSqlColumnType() || node.Arguments[i].NodeType == ExpressionType.New || node.Arguments[i].NodeType == ExpressionType.Conditional || node.Arguments[i].NodeType==ExpressionType.Parameter)
               {
                  arguments.Add(Visit(node.Arguments[i]));
               }
               else
               {
                  var b = new SqlExpressionVisitor();
                  var ctx = new JoinBuilderContext(_formatter, _joins);
                  b.BuildSql(ctx, node.Arguments[i]);
                  var sql = ctx.ToString();
                  var index = AddOrGetColumnIndex(sql,members[i]);
                  arguments.Add(GetReaderExpression(members[i].GetMemberType(), index));
                  _memberMap[members[i]] = new MappedMemberInfo(members[i],node.Arguments[i],sql,Aliases.Column(index));
               }
            }
            return Expression.New(node.Constructor,arguments,node.Members);
         }
         return base.VisitNew(node);
      }

      protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
      {
         var e = node.Expression;
         if (!e.Type.IsSqlColumnType() || e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.Conditional ||e.NodeType == ExpressionType.Parameter)
         {
            return base.VisitMemberAssignment(node);
         }
         var b = new SqlExpressionVisitor();
         var ctx = new JoinBuilderContext(_formatter, _joins);
         b.BuildSql(ctx, e);
         var sql = ctx.ToString();
         var index = AddOrGetColumnIndex(ctx.ToString(),node.Member);
         e = GetReaderExpression(node.Member.GetMemberType(), index);
         _memberMap[node.Member] = new MappedMemberInfo(node.Member, node.Expression, sql, Aliases.Column(index));
         return node.Update(e);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         Expression substitute;
         var substituter = new CreateExpressionSubstituteVisitor();
         if (substituter.TrySubstituteFactoryMethod(node, _origParameter, out substitute))
         {
            return Visit(substitute);
         }
         if (node.IsJoinMethod())
         {
            var joinPath = node.GetJoinPath();
            _joins.Add(joinPath);
            return node;
         }
         return base.VisitMethodCall(node);
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         if (!node.Expression.IsParameter() && !node.Expression.IsJoinMethod())
         {
            return base.VisitMember(node);
         }

         var b = new SqlExpressionVisitor();
         var ctx = new JoinBuilderContext(_formatter, _joins);
         b.BuildSql(ctx, node);
         var index = AddOrGetColumnIndex(ctx.ToString(), node.Member);
         return GetReaderExpression(node.Member.GetMemberType(), index);


         //var expression = _formatter.FormatColumn(node.Member.GetColumnDescriptor());
         //var index = AddOrGetColumnIndex(expression,null);
         //return GetReaderExpression(node.Member.GetMemberType(), index);
      }

      private int AddOrGetColumnIndex(string sqlExpression,MemberInfo mappedMember)
      {
         ColumnInfo currentColumnInfo = null;
         int? index = null;
         foreach (var c in _columns.Values)
         {
            if (c.Sql == sqlExpression)
            {
               index = c.Index;
               if (c.MappedMember == null)
               {
                  currentColumnInfo = c;
               }
               if (mappedMember == null)
               {
                  return c.Index;
               }
               break;
            }
         }
         if (currentColumnInfo != null)
         {
            currentColumnInfo.MappedMember = mappedMember;
            return currentColumnInfo.Index;
         }
         var newIndex = index ?? (_columns.Count == 0 ? 0 : _columns.Keys.Select(c => c.Index).Max() + 1);
         var columnInfo = new ColumnInfo(sqlExpression, newIndex, mappedMember);
         ColumnInfo found;
         if(_columns.TryGetValue(columnInfo,out found))
         {
            return found.Index;
         }
         _columns.Add(columnInfo,columnInfo);
         return columnInfo.Index;
      }

      private Expression GetReaderExpression(Type type, int index)
      {
         var readerMethod = GetGetterMethod(type);
         var resultExpression = readerMethod.DeclaringType == typeof(IDataRecord)
                    ? Expression.Call(_parameter, readerMethod, Expression.Constant(index, typeof(int)))
                    : Expression.Call(readerMethod, _parameter, Expression.Constant(index, typeof(int)));
         return readerMethod.ReturnType != type 
            ? (Expression) Expression.Convert(resultExpression, type) 
            : resultExpression;
      }
     
      private static MethodInfo GetGetterMethod(Type type)
      {
         var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
         var name = nonNullableType == typeof(Single) ? "Float" : nonNullableType.Name;
         if (!type.IsValueType || type.IsNullable())
         {
            name = "GetN" + name;
         }
         else
         {
            name = "Get" + name;
         }

         return !type.IsValueType || type.IsNullable()
            ? (typeof (NullableGetters).GetMethod(name) ?? typeof (NullableGetters).GetMethod("GetNValue"))
            : (typeof (IDataRecord).GetMethod(name) ?? typeof (NullableGetters).GetMethod("GetNValue"));
      }
   }
}
