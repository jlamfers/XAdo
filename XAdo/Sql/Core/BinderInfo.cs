using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XAdo.Sql.Core
{
   public class BinderInfo
   {
      private class BinderVisitor : ExpressionVisitor
      {
         public Dictionary<MemberInfo, Tuple<Expression, int>> Bindings = new Dictionary<MemberInfo, Tuple<Expression, int>>();

         public BinderVisitor BuildBindings(Expression expression)
         {
            Visit(expression);
            return this;
         }

         // anonymous new expression
         protected override Expression VisitNew(NewExpression node)
         {
            if (node.Arguments != null && node.Arguments.Any())
            {
               for (var i = 0; i < node.Arguments.Count; i++)
               {
                  Bindings[_current = node.Members[i]] = Tuple.Create(node.Arguments[i], i);
               }
               return node;
            }
            return base.VisitNew(node);
         }

         private MemberInfo _current;
         protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
         {
            Bindings[_current = node.Member] = Tuple.Create(node.Expression, 0);
            return base.VisitMemberAssignment(node);
         }

         protected override Expression VisitConstant(ConstantExpression node)
         {
            if (node.Value is int)
            {
               var b = Bindings[_current];
               Bindings[_current] = Tuple.Create(b.Item1, (int) node.Value);
            }
            return base.VisitConstant(node);
         }
      }

      private class MapVisitor : ExpressionVisitor
      {
         private SelectInfo _selectInfo;
         private BinderInfo _binderInfo;
         private ParameterExpression _parameter;
         public readonly List<ColumnInfo> Columns = new List<ColumnInfo>();

         public LambdaExpression Substitute(LambdaExpression mapExpression, SelectInfo selectInfo, BinderInfo binderInfo)
         {
            _selectInfo = selectInfo;
            _binderInfo = binderInfo;
            _parameter = Expression.Parameter(typeof (IDataRecord), "row");
            var body = Visit(mapExpression.Body);
            return Expression.Lambda(body,_parameter);
         }

         protected override Expression VisitNew(NewExpression node)
         {
            if (node.Arguments == null || !node.Arguments.Any())
            {
               return base.VisitNew(node);
            }
            var args = new List<Expression>();
            for (var j = 0; j < node.Arguments.Count; j++)
            {
               if (node.Arguments[j].NodeType == ExpressionType.New)
               {
                  args.Add(Visit(node.Arguments[j]));
                  continue;
               }
               var member = node.Arguments[j].GetMemberInfo(false);
               int index;
               if (member == null || !_binderInfo.BindingIndices.TryGetValue(member, out index))
               {
                  args.Add(Visit(node.Arguments[j]));
                  continue;
               }

               var column = _selectInfo.Columns[index];
               var newIndex = Columns.FindIndex(c => c.Index == column.Index);
               if (newIndex == -1)
               {
                  newIndex = Columns.Count;
                  var c = column.Clone();
                  c.MappedMember = node.Members[j];
                  Columns.Add(c);
               }
               args.Add(BinderFactory.GetRecordGetter(node.Members[j], newIndex, _parameter, column.NotNull));
            }
            return Expression.New(node.Constructor, args, node.Members);
         }

         protected override MemberBinding VisitMemberBinding(MemberBinding node)
         {
        
            int index;
            if (_binderInfo.BindingIndices.TryGetValue(node.Member, out index))
            {
               var column = _selectInfo.Columns[index];
               var newIndex = Columns.FindIndex(c => c.Index == column.Index);
               if (newIndex == -1)
               {
                  newIndex = Columns.Count;
                  var c = column.Clone();
                  c.MappedMember = node.Member;
                  Columns.Add(c);
               }
               return Expression.MemberBind(node.Member, BinderFactory.GetMemberBinder(node.Member, newIndex, _parameter, column.NotNull));
            }
           return base.VisitMemberBinding(node);
         }

      }

      public BinderInfo(Expression binderExpression)
      {
         BinderExpression = binderExpression;
         var bindings = new BinderVisitor().BuildBindings(binderExpression).Bindings;
         BindingExpressions = bindings.ToDictionary(b => b.Key, b => b.Value.Item1).AsReadOnly();
         BindingIndices = bindings.ToDictionary(b => b.Key, b => b.Value.Item2).AsReadOnly();
      }

      public Expression BinderExpression { get; private set; }
      public IDictionary<MemberInfo, Expression> BindingExpressions { get; private set; }
      public IDictionary<MemberInfo, int> BindingIndices { get; private set; }

      public LambdaExpression Map(LambdaExpression mappedBinderExpression, SelectInfo selectInfo, out SelectInfo mappedSelectInfo)
      {
         var mapVisitor = new MapVisitor();
         var binderExpression = mapVisitor.Substitute(mappedBinderExpression, selectInfo, this);
         mappedSelectInfo = selectInfo.Map(mapVisitor.Columns);
         return binderExpression;
      }
   }
}