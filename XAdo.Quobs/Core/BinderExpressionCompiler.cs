using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core
{
   public class BinderExpressionCompiler : ExpressionVisitor
   {
      private readonly ISqlFormatter _formatter;

      public static class NullableGetters
      {
         public static object GetValue(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? null : reader.GetValue(index);
         }
         public static string GetString(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? null : reader.GetString(index);
         }
         public static Byte? GetByte(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Byte?)null : reader.GetByte(index);
         }

         public static Boolean? GetBoolean(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Boolean?)null : reader.GetBoolean(index);
         }

         public static Char? GetChar(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Char?)null : reader.GetChar(index);
         }

         public static Decimal? GetDecimal(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Decimal?)null : reader.GetDecimal(index);
         }

         public static Double? GetDouble(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Double?)null : reader.GetDouble(index);
         }

         public static Single? GetFloat(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Single?)null : reader.GetFloat(index);
         }

         public static Guid? GetGuid(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Guid?)null : reader.GetGuid(index);
         }

         public static Int16? GetInt16(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Int16?)null : reader.GetInt16(index);
         }

         public static Int32? GetInt32(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Int32?)null : reader.GetInt32(index);
         }

         public static Int64? GetInt64(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (Int64?)null : reader.GetInt64(index);
         }

         public static DateTime? GetDateTime(IDataRecord reader, int index)
         {
            return reader.IsDBNull(index) ? (DateTime?)null : reader.GetDateTime(index);
         }
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
            get { return "__c_" + Index; }
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
         [Obsolete]
         public List<QueryDescriptor.JoinDescriptor> Joins { get; set; }
         public Dictionary<MemberInfo, Expression> MemberToExpressionMap { get; set; }
         public ParameterExpression OrigParameter { get; set; }
         public Expression<Func<IDataRecord, T>> BinderExpression { get; set; }
      }

      private ParameterExpression
         _parameter;

      private Dictionary<ColumnInfo, ColumnInfo>
         _columns;

      private List<DbSchemaDescriptor.JoinPath>
         _joins;

      private Dictionary<MemberInfo, Expression>
         _memberMap;

      private ParameterExpression
         _origParameter;

      public BinderExpressionCompiler(ISqlFormatter formatter)
      {
         _formatter = formatter;
      }

      public CompileResult<T> Compile<T>(LambdaExpression expression)
      {
         _parameter = Expression.Parameter(typeof(IDataRecord), "rdr");
         _columns = new Dictionary<ColumnInfo, ColumnInfo>();
         _joins = new List<DbSchemaDescriptor.JoinPath>();
         _memberMap = new Dictionary<MemberInfo, Expression>();
         _origParameter = expression.Parameters.Single();
         var body = Visit(expression.Body);
         var binderExpression = Expression.Lambda<Func<IDataRecord, T>>(body, _parameter);
         return new CompileResult<T>
         {
            BinderExpression = binderExpression,
            Joins = _joins.SelectMany(j => j.Joins).Select(j =>  new QueryDescriptor.JoinDescriptor(j.JoinInfo.Format(_formatter.IdentifierDelimiterLeft,_formatter.IdentifierDelimiterRight),j.JoinType.ToJoinTypeString())).ToList(),
            Columns = _columns.Keys.OrderBy(c => c.Index).ToList(),
            MemberToExpressionMap = _memberMap,
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
               _memberMap[members[i]] = node.Arguments[i];
               if (!node.Arguments[i].Type.IsSqlColumnType() || node.Arguments[i].NodeType == ExpressionType.New || node.Arguments[i].NodeType == ExpressionType.Conditional || node.Arguments[i].NodeType==ExpressionType.Parameter)
               {
                  arguments.Add(Visit(node.Arguments[i]));
               }
               else
               {
                  var b = new SqlExpressionBuilder();
                  var ctx = new QuobContext(_formatter, _joins);
                  b.BuildSql(ctx, node.Arguments[i]);
                  var index = AddOrGetColumnIndex(ctx.ToString(),members[i]);
                  arguments.Add(GetReaderExpression(members[i].GetMemberType(), index));
               }
            }
            return Expression.New(node.Constructor,arguments,node.Members);
         }
         return base.VisitNew(node);
      }

      protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
      {
         _memberMap[node.Member] = node.Expression;
         var e = node.Expression;
         if (!e.Type.IsSqlColumnType() || e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.Conditional ||e.NodeType == ExpressionType.Parameter)
         {
            return base.VisitMemberAssignment(node);
         }
         var b = new SqlExpressionBuilder();
         var ctx = new QuobContext(_formatter, _joins);
         b.BuildSql(ctx, e);
         var index = AddOrGetColumnIndex(ctx.ToString(),node.Member);
         e = GetReaderExpression(node.Member.GetMemberType(), index);
         return node.Update(e);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
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

         var expression = _formatter.FormatColumn(node.Member.GetColumnDescriptor());
         var index = AddOrGetColumnIndex(expression,null);
         return GetReaderExpression(node.Member.GetMemberType(), index);
      }

      private int AddOrGetColumnIndex(string sqlExpression,MemberInfo mappedMember)
      {
         ColumnInfo currentColumnInfo = null;
         foreach (var c in _columns.Values)
         {
            if (c.Sql == sqlExpression)
            {
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
         var columnInfo = new ColumnInfo(sqlExpression, _columns.Count, mappedMember);
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
         var name = "Get" + (nonNullableType == typeof(Single) ? "Float" : nonNullableType.Name);
         return !type.IsValueType || type.IsNullable()
            ? (typeof (NullableGetters).GetMethod(name) ?? typeof (NullableGetters).GetMethod("GetValue"))
            : (typeof (IDataRecord).GetMethod(name) ?? typeof (NullableGetters).GetMethod("GetValue"));
      }
   }
}
