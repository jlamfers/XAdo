using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Core;
using XAdo.Quobs.Attributes;
using XAdo.Quobs.Meta;

namespace XAdo.Quobs.Expressions
{
   public class BinderExpressionCompiler : ExpressionVisitor
   {
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

      private class OrderedColumnDescriptor
      {
         public int Order;
         public SchemaDescriptor.ColumnDescriptor Descriptor;
      }

      public class CompileBaseResult
      {
         public List<SchemaDescriptor.ColumnDescriptor> Columns { get; set; }
         public List<SchemaDescriptor.JoinDescriptor> Joins { get; set; }
         public virtual Expression BinderExpression { get; set; }
         
      }
      public class CompileResult : CompileBaseResult
      {
         public new Expression<Func<IDataRecord, object>> BinderExpression
         {
            set { base.BinderExpression = value; }
            get { return (Expression<Func<IDataRecord, object>>)base.BinderExpression; }
         }
      }
      public class CompileResult<T> : CompileBaseResult
      {
         public new Expression<Func<IDataRecord, T>> BinderExpression
         {
            set { base.BinderExpression = value; }
            get { return (Expression<Func<IDataRecord, T>>)base.BinderExpression; }
         }
      }

      private ParameterExpression
         _parameter;

      private Dictionary<MemberInfo, OrderedColumnDescriptor>
         _columns;

      private Dictionary<string,SchemaDescriptor.JoinDescriptor>
         _joins;

      public CompileResult Compile(LambdaExpression expression)
      {
         _parameter = Expression.Parameter(typeof (IDataRecord),"rdr");
         _columns = new Dictionary<MemberInfo, OrderedColumnDescriptor>();
         _joins = new Dictionary<string, SchemaDescriptor.JoinDescriptor>();
         var body = Expression.Convert(Visit(expression.Body),typeof(object));
         var binderExpression = Expression.Lambda<Func<IDataRecord, object>>(body, _parameter);
         return new CompileResult
         {
            BinderExpression = binderExpression,
            Joins = _joins.Values.ToList(),
            Columns = _columns.Values.OrderBy(c => c.Order).Select(c => c.Descriptor).ToList()
         };
      }
      public CompileResult<T> Compile<T>(LambdaExpression expression)
      {
         _parameter = Expression.Parameter(typeof(IDataRecord), "rdr");
         _columns = new Dictionary<MemberInfo, OrderedColumnDescriptor>();
         _joins = new Dictionary<string, SchemaDescriptor.JoinDescriptor>();
         var body = Visit(expression.Body);
         var binderExpression = Expression.Lambda<Func<IDataRecord, T>>(body, _parameter);
         return new CompileResult<T>
         {
            BinderExpression = binderExpression,
            Joins = _joins.Values.ToList(),
            Columns = _columns.Values.OrderBy(c => c.Order).Select(c => c.Descriptor).ToList()
         };
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
         return _parameter;
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         if (node.IsJoinMethod())
         {
            foreach (var jd in node.GetJoinDescriptors())
            {
               if (!_joins.ContainsKey(jd.Expression))
               {
                  _joins.Add(jd.Expression, jd);
               }
            }
            return Visit(node.Arguments[0]);
         }
         return base.Visit(node);
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         if (!node.Expression.IsParameter() && !node.Expression.IsJoinMethod())
         {
            return base.VisitMember(node);
         }

         // add column
         if (!_columns.ContainsKey(node.Member))
         {
            _columns.Add(node.Member, new OrderedColumnDescriptor { Descriptor = node.Member.GetColumnDescriptor() , Order = _columns.Count});
         }

         var memberType = node.Member.GetMemberType();
         var readerMethod = GetReaderMethod(_columns[node.Member].Descriptor);
         var result = readerMethod.DeclaringType == typeof (IDataRecord)
            ? Expression.Call(Visit(node.Expression), readerMethod,Expression.Constant(_columns[node.Member].Order, typeof (int)))
            : Expression.Call(readerMethod, Visit(node.Expression),Expression.Constant(_columns[node.Member].Order, typeof (int)));
         Visit(node.Expression);
         return readerMethod.ReturnType != memberType ? (Expression) Expression.Convert(result, memberType) : result;
      }

      private static MethodInfo GetReaderMethod(SchemaDescriptor.ColumnDescriptor d)
      {
         var type = d.Member.GetMemberType();
         if (d.Required || ( type.IsValueType && !type.IsNullable()))
         {
            var m = typeof (IDataRecord).GetMethod(GetGetterName(type));
            if (m != null) return m;
         }
         if (!type.IsValueType || type.IsNullable())
         {
            var m = typeof (NullableGetters).GetMethod(GetGetterName(Nullable.GetUnderlyingType(type) ?? type));
            if (m != null) return m;
         }
         return typeof (NullableGetters).GetMethod("GetValue");
      }
     

      private static string GetGetterName(Type type)
      {
         return "Get" + (type == typeof(Single) ? "Float" : type.Name);
      }
   }
}
