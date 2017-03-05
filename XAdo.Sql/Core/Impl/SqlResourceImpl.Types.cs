using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Mapper;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Impl
{
   // immutable object
   public partial class SqlResourceImpl
   {
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
            _parameter = Expression.Parameter(typeof (IDataRecord), "row");
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
                  _toColumns.Add(Tuple.Create(c, (MemberInfo) null));
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
               var t = (Func<IDataRecord, TEntity>) @delegate;
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
               return _binderDelegate ?? (_binderDelegate = typeof (Compiler<>)
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
                  return _objectBinderDelegate ?? (_objectBinderDelegate = typeof (Compiler<>)
                     .MakeGenericType(BinderExpression.Body.Type)
                     .CreateInstance()
                     .CastTo<ICompiler>()
                     .CastToObjectBinder(BinderDelegate));
               }
            }
         }

         public Type EntityType { get { return BinderExpression.Body.Type; } }
      }
   }

}