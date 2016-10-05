using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Expressions;

namespace XAdo.Quobs.Sql
{
   internal class SqlNewExpressionCompiler : ExpressionVisitor
   {

      private List<Tuple<MemberInfo, MemberInfo>> _columns;
      private ReadOnlyCollection<MemberInfo> _members;
      private int _memberIndex;

      public List<Tuple<MemberInfo, MemberInfo>> Compile(Expression expression)
      {
         _columns = new List<Tuple<MemberInfo, MemberInfo>>();
         Visit(expression);
         return _columns;
      }

      protected override Expression VisitUnary(UnaryExpression node)
      {
         switch (node.NodeType)
         {
            case ExpressionType.Convert:
               Visit(node.Operand);
               break;
            default:
               throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
         }
         return node;
      }

      protected override Expression VisitNew(NewExpression node)
      {
         _members = node.Members;
         return base.VisitNew(node);
      }

      protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
      {
         // with known type and member assignments
         _columns.Add(Tuple.Create(node.Expression.GetMemberInfo(), node.Member));
         return node;
      }

      protected override Expression VisitBinary(BinaryExpression node)
      {
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
         {
            // with anonymous type
            _columns.Add(Tuple.Create(node.Member, _members[_memberIndex++]));
            return node;
         }
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }
      protected override Expression VisitConstant(ConstantExpression node)
      {
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }
      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }

   }
}