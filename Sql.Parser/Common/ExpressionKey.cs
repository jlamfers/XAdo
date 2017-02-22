using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sql.Parser.Common
{


   internal class ExpressionKey : ExpressionVisitor
   {

      private List<byte> _bytes;

      public string BuildKey(Expression node)
      {
         _bytes = new List<byte>(10000);
         Visit(node);
         var result = Convert.ToBase64String(_bytes.ToArray());
         _bytes = null;
         return result;
      }

      protected override Expression VisitBinary(BinaryExpression node)
      {
         Write(node);
         return base.VisitBinary(node);
      }

      protected override Expression VisitBlock(BlockExpression node)
      {
         Write(node);
         return base.VisitBlock(node);
      }

      protected override CatchBlock VisitCatchBlock(CatchBlock node)
      {
         Write(node);
         return base.VisitCatchBlock(node);
      }

      protected override Expression VisitConditional(ConditionalExpression node)
      {
         Write(node);
         return base.VisitConditional(node);
      }

      protected override Expression VisitConstant(ConstantExpression node)
      {
         Write(node);
         Write(node.Value);
         return base.VisitConstant(node);
      }

      protected override Expression VisitDebugInfo(DebugInfoExpression node)
      {
         Write(node);
         return base.VisitDebugInfo(node);
      }

      protected override Expression VisitDefault(DefaultExpression node)
      {
         Write(node);
         return base.VisitDefault(node);
      }

      protected override Expression VisitDynamic(DynamicExpression node)
      {
         Write(node);
         Write(node.DelegateType);
         return base.VisitDynamic(node);
      }

      protected override Expression VisitExtension(Expression node)
      {
         Write(node);
         Write(node.CanReduce);
         return base.VisitExtension(node);
      }

      protected override ElementInit VisitElementInit(ElementInit node)
      {
         Write(node);
         Write(node.AddMethod);
         return base.VisitElementInit(node);
      }

      protected override Expression VisitGoto(GotoExpression node)
      {
         Write(node);
         return base.VisitGoto(node);
      }

      protected override Expression VisitIndex(IndexExpression node)
      {
         Write(node);
         return base.VisitIndex(node);
      }

      protected override Expression VisitInvocation(InvocationExpression node)
      {
         Write(node);
         return base.VisitInvocation(node);
      }

      protected override Expression VisitLabel(LabelExpression node)
      {
         Write(node);
         return base.VisitLabel(node);
      }

      protected override Expression VisitLambda<T>(Expression<T> node)
      {
         Write(node);
         return base.VisitLambda(node);
      }

      protected override Expression VisitListInit(ListInitExpression node)
      {
         Write(node);
         return base.VisitListInit(node);
      }

      protected override Expression VisitLoop(LoopExpression node)
      {
         Write(node);
         return base.VisitLoop(node);
      }

      protected override Expression VisitMember(MemberExpression node)
      {
         Write(node);
         Write(node.Member);
         return base.VisitMember(node);
      }

      protected override Expression VisitMemberInit(MemberInitExpression node)
      {
         Write(node);
         return base.VisitMemberInit(node);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         Write(node);
         Write(node.Method);
         return base.VisitMethodCall(node);
      }

      protected override Expression VisitNew(NewExpression node)
      {
         Write(node);
         if (node.Members != null)
         {
            foreach (var m in node.Members)
            {
               Write(m);
            }
         }
         return base.VisitNew(node);
      }

      protected override Expression VisitNewArray(NewArrayExpression node)
      {
         Write(node);
         return base.VisitNewArray(node);
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
         Write(node);
         Write(node.Type);
         Write(node.IsByRef);
         return base.VisitParameter(node);
      }

      protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
      {
         Write(node);
         return base.VisitRuntimeVariables(node);
      }

      protected override Expression VisitSwitch(SwitchExpression node)
      {
         Write(node);
         return base.VisitSwitch(node);
      }

      protected override Expression VisitTry(TryExpression node)
      {
         Write(node);
         return base.VisitTry(node);
      }

      protected override Expression VisitTypeBinary(TypeBinaryExpression node)
      {
         Write(node);
         return base.VisitTypeBinary(node);
      }

      protected override Expression VisitUnary(UnaryExpression node)
      {
         Write(node);
         Write(node.Method);
         Write(node.IsLifted);
         Write(node.IsLiftedToNull);
         return base.VisitUnary(node);
      }

      protected override LabelTarget VisitLabelTarget(LabelTarget node)
      {
         Write(node.Name);
         return base.VisitLabelTarget(node);
      }

      protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
      {
         Write(node.BindingType);
         return base.VisitMemberAssignment(node);
      }

      protected override MemberBinding VisitMemberBinding(MemberBinding node)
      {
         Write(node.BindingType);
         Write(node.Member);
         return base.VisitMemberBinding(node);
      }

      protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
      {
         Write(node.BindingType);
         Write(node.Member);
         return base.VisitMemberListBinding(node);
      }

      protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
      {
         Write(node.BindingType);
         return base.VisitMemberMemberBinding(node);
      }

      private void Write(Expression node)
      {
         _bytes.Add((byte) (int) node.NodeType);
         WriteHashCode(node.Type.GetHashCode());
      }

      private void Write(MemberBindingType item)
      {
         _bytes.Add((byte) (int) item);
      }

      private void Write(object item)
      {
         WriteHashCode(item.GetHashCode());
      }

      private void WriteHashCode(int hash)
      {
         var b1 = (byte) (hash >> 24);
         var b2 = (byte) (hash >> 16);
         var b3 = (byte) (hash >> 8);
         var b4 = (byte) hash;
         if (b1 != 0)
         {
            _bytes.Add(b1);
            _bytes.Add(b2);
            _bytes.Add(b3);
            _bytes.Add(b4);
            return;
         }
         if (b2 != 0)
         {
            _bytes.Add(b2);
            _bytes.Add(b3);
            _bytes.Add(b4);
            return;
         }
         if (b3 != 0)
         {
            _bytes.Add(b3);
            _bytes.Add(b4);
            return;
         }
         _bytes.Add(b4);
      }
   }

   public static class ExpressionKeyExtension
   {

      private static readonly LRUCache<string,object> 
         Cache = new LRUCache<string, object>(1000);

      public static string GetKey(this Expression self)
      {
         return self == null ? null : new ExpressionKey().BuildKey(self);
      }

      public static Func<T1, T2, T3, T4, T5, T6, T7, T8> CompileCached<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8>> self)
      {
         return (Func<T1, T2, T3, T4, T5, T6, T7, T8>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T1, T2, T3, T4, T5, T6, T7> CompileCached<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7>> self)
      {
         return (Func<T1, T2, T3, T4, T5, T6, T7>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T1, T2, T3, T4, T5, T6> CompileCached<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6>> self)
      {
         return (Func<T1, T2, T3, T4, T5, T6>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T1, T2, T3, T4, T5> CompileCached<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5>> self)
      {
         return (Func<T1, T2, T3, T4, T5>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T1, T2, T3, T4> CompileCached<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4>> self)
      {
         return (Func<T1, T2, T3, T4>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T1, T2, T3> CompileCached<T1, T2, T3>(this Expression<Func<T1, T2, T3>> self)
      {
         return (Func<T1, T2, T3>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T1, T2> CompileCached<T1, T2>(this Expression<Func<T1, T2>> self)
      {
         return (Func<T1, T2>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Func<T> CompileCached<T>(this Expression<Func<T>> self)
      {
         return (Func<T>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }


      public static Action<T1, T2, T3, T4, T5, T6, T7, T8> CompileCached<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Action<T1, T2, T3, T4, T5, T6, T7, T8>> self)
      {
         return (Action<T1, T2, T3, T4, T5, T6, T7, T8>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T1, T2, T3, T4, T5, T6, T7> CompileCached<T1, T2, T3, T4, T5, T6, T7>(this Expression<Action<T1, T2, T3, T4, T5, T6, T7>> self)
      {
         return (Action<T1, T2, T3, T4, T5, T6, T7>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T1, T2, T3, T4, T5, T6> CompileCached<T1, T2, T3, T4, T5, T6>(this Expression<Action<T1, T2, T3, T4, T5, T6>> self)
      {
         return (Action<T1, T2, T3, T4, T5, T6>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T1, T2, T3, T4, T5> CompileCached<T1, T2, T3, T4, T5>(this Expression<Action<T1, T2, T3, T4, T5>> self)
      {
         return (Action<T1, T2, T3, T4, T5>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T1, T2, T3, T4> CompileCached<T1, T2, T3, T4>(this Expression<Action<T1, T2, T3, T4>> self)
      {
         return (Action<T1, T2, T3, T4>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T1, T2, T3> CompileCached<T1, T2, T3>(this Expression<Action<T1, T2, T3>> self)
      {
         return (Action<T1, T2, T3>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T1, T2> CompileCached<T1, T2>(this Expression<Action<T1, T2>> self)
      {
         return (Action<T1, T2>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action<T> CompileCached<T>(this Expression<Action<T>> self)
      {
         return (Action<T>)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }
      public static Action CompileCached(this Expression<Action> self)
      {
         return (Action)Cache.GetOrAdd(self.GetKey(), k => self.Compile());
      }

   }

}