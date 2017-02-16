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
                  if (!HandleCall(node.Members[i], node.Arguments[i].Trim() as MethodCallExpression))
                  {
                     Visit(node.Arguments[i]);
                  }
               }
               return node;
            }
            return base.VisitNew(node);
         }

         protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
         {
            return HandleCall(node.Member, node.Expression.Trim() as MethodCallExpression) ? node : base.VisitMemberAssignment(node);
         }

         private bool HandleCall(MemberInfo member, MethodCallExpression call)
         {
            if (call != null)
            {
               var target = (call.Object ?? call.Arguments.FirstOrDefault());
               if (target != null && target.Type == typeof(IDataRecord))
               {
                  var constant = call.Arguments.Last() as ConstantExpression;
                  if (constant != null && constant.Value is int)
                  {
                     Bindings[member] = Tuple.Create((Expression)call, (int)constant.Value);
                     return true;
                  }
               }
            }
            return false;
         }
      }

      private class MapVisitor : ExpressionVisitor
      {
         private SqlSelectInfo _selectInfo;
         private BinderInfo _binderInfo;
         private ParameterExpression _parameter;
         public readonly List<ColumnInfo> Columns = new List<ColumnInfo>();

         public LambdaExpression Substitute(LambdaExpression mapExpression, SqlSelectInfo selectInfo, BinderInfo binderInfo)
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
            for (var i = 0; i < node.Arguments.Count; i++)
            {
               if (node.Arguments[i].NodeType == ExpressionType.New)
               {
                  args.Add(Visit(node.Arguments[i]));
                  continue;
               }
               var member = node.Arguments[i].GetMemberInfo(false);
               int index;
               if (member == null || !_binderInfo.BindingIndices.TryGetValue(member, out index))
               {
                  args.Add(Visit(node.Arguments[i]));
                  continue;
               }

               var column = _selectInfo.Columns[index];
               //note: indices are reset later
               var newIndex = Columns.FindIndex(c => c.Index == column.Index);
               if (newIndex == -1)
               {
                  newIndex = Columns.Count;
                  var c = column.Clone();
                  c.MappedMember = node.Members[i];
                  Columns.Add(c);
               }
               args.Add(node.Members[i].GetDataRecordRecordGetterExpression(newIndex, _parameter, column.NotNull));
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
               return Expression.MemberBind(node.Member, node.Member.GetDataRecordMemberAssignmentExpression(newIndex, _parameter, column.NotNull));
            }
           return base.VisitMemberBinding(node);
         }

         protected override Expression VisitMember(MemberExpression node)
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
                  c.MappedMember = null;
                  Columns.Add(c);
               }
               return node.Member.GetDataRecordRecordGetterExpression(newIndex, _parameter, column.NotNull);
            }
            return base.VisitMember(node);
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

      public LambdaExpression Map(LambdaExpression mappedBinderExpression, SqlSelectInfo selectInfo, out SqlSelectInfo mappedSelectInfo)
      {
         var mappedType = mappedBinderExpression.Body.Type;
         var mapVisitor = new MapVisitor();
         var binderExpression = mapVisitor.Substitute(mappedBinderExpression, selectInfo, this);
         var memberToPathMap = mappedType.GetMemberPathMap();
         foreach (var c in mapVisitor.Columns)
         {
            if (c.MappedMember == null)
            {
               c.FullName = null;
               c.Path = null;
               c.Name = null;
            }
            else
            {
               c.FullName = memberToPathMap[c.MappedMember];
               c.Name = c.FullName.Split('.').Last();
               c.Path = c.FullName.Contains('.') ? c.FullName.Substring(0, c.FullName.LastIndexOf('.')) : "";
               c.MappedMember = null;
            }
         }
         mappedSelectInfo = selectInfo.Map(mapVisitor.Columns);
         return binderExpression;
      }
   }
}