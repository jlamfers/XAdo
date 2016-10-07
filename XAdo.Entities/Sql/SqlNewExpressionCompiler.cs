using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Attributes;
using XAdo.Quobs.Expressions;

namespace XAdo.Quobs.Sql
{
   internal class SqlNewExpressionCompiler : ExpressionVisitor
   {

      public class CompileResult
      {
         public CompileResult(List<Tuple<MemberInfo, MemberInfo>> tuples, IDictionary<string, JoinType?> joins)
         {
            Joins = joins;
            Tuples = tuples;
         }

         public List<Tuple<MemberInfo, MemberInfo>> Tuples { get; private set; }
         public IDictionary<string, JoinType?> Joins { get; private set; }
        
      }

      private Dictionary<string, JoinType?>
         _joins;


      private List<Tuple<MemberInfo, MemberInfo>> _columns;
      private ReadOnlyCollection<MemberInfo> _members;
      private int _memberIndex;

      public CompileResult Compile(Expression expression)
      {
         _columns = new List<Tuple<MemberInfo, MemberInfo>>();
         _joins = new Dictionary<string, JoinType?>();
         Visit(expression);
         return new CompileResult(_columns,_joins);
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
         if (node.Expression.CastTo<MemberExpression>().Expression.NodeType != ExpressionType.Parameter)
         {
            Visit(node.Expression.CastTo<MemberExpression>().Expression);
         }
         return node;
      }

      protected override Expression VisitBinary(BinaryExpression node)
      {
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         bool joined = false;
         if (node.Expression != null && (node.Expression.NodeType == ExpressionType.Parameter || (joined = node.Expression.NodeType == ExpressionType.Call && node.Expression.CastTo<MethodCallExpression>().Method.GetCustomAttribute<JoinMethodAttribute>() != null)))
         {
            // with anonymous type
            _columns.Add(Tuple.Create(node.Member, _members[_memberIndex++]));
            return joined ? base.VisitMember(node) : node;
         }
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }
      protected override Expression VisitConstant(ConstantExpression node)
      {
         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }
      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         JoinMethodAttribute att;
         if ((att = node.Method.GetCustomAttribute<JoinMethodAttribute>()) != null)
         {
            var joinType = node.Arguments.Count > 1 ? (JoinType?) node.Arguments[1].GetExpressionValue() : null;
            if (!_joins.ContainsKey(att.Expression))
            {
               _joins[att.Expression] = joinType;
            }
            Visit(node.Arguments[0]);
            return node;
         }

         throw new NotSupportedException(string.Format("Expression not supported: {0}", node));
      }

   }
}