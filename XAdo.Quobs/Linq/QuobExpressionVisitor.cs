using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression.Core;

namespace XAdo.Quobs.Linq
{
   internal class QuobExpressionVisitor : ExpressionVisitor
   {
      public IQuob Quob { get; private set; }

      private readonly Stack<Func<IQuob, object>> _stack = new Stack<Func<IQuob, object>>();

      public QuobExpressionVisitor(IQuob quob)
      {
         Quob = quob;
      }

      public object Traverse(Expression expression)
      {
         object result = null;
         Visit(expression);
         while (_stack.Any())
         {
            result = _stack.Pop()(Quob);
         }
         if (result is IQuob)
         {
            result = null;
         }
         return result;
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         if (node.Method.DeclaringType != typeof (Queryable))
         {
            return base.VisitMethodCall(node);
         }

         switch (node.Method.Name)
         {
            case "Where":
               _stack.Push(q => q.Where(node.Arguments[1]));
               break;

            case "Count":
               _stack.Push(q => q.Count());
               break;

            case "Any":
               _stack.Push(q => q.Any());
               break;

            case "Skip":
               _stack.Push(q =>
               {
                  object value;
                  node.Arguments[1].TryEvaluate(out value);
                  q.Skip((int) value);
                  return q;
               });
               break;

            case "Take":
               _stack.Push(q =>
               {
                  object value;
                  node.Arguments[1].TryEvaluate(out value);
                  q.Take((int) value);
                  return q;
               });
               break;

            case "OrderBy":
               _stack.Push(q => Quob = q.OrderBy(false,false, node.Arguments[1]));
               break;

            case "OrderByDescending":
               _stack.Push(q => Quob = q.OrderBy(false, true, node.Arguments[1]));
               break;

            case "ThenBy":
               _stack.Push(q => Quob = q.OrderBy(true, false, node.Arguments[1]));
               break;

            case "ThenByDescending":
               _stack.Push(q => Quob = q.OrderBy(true, true, node.Arguments[1]));
               break;

            case "Select":
               _stack.Push(q =>
               {
                  var lambda = (LambdaExpression) node.Arguments[1].Unquote();
                  if (lambda.Body.NodeType != ExpressionType.Parameter)
                  {
                     // only call for specific selections, not for the parameter itself (like p => p), which is the default anyway
                     Quob = q.Select(lambda);
                  }
                  return Quob;
               });
               break;

            case "Distinct":
               _stack.Push(q => q.Distinct());
               break;

            case "Single":
               _stack.Push(q =>
               {
                  var e = q.ToEnumerable().GetEnumerator();
                  if (!e.MoveNext() )
                  {
                     throw new InvalidOperationException("No items in collection");
                  }
                  var result = e.Current;
                  if (e.MoveNext())
                  {
                     throw new InvalidOperationException("More than one item in collection");
                  }
                  return result;
               });
               //_quob.Single();
               break;

            default:
               throw new QuobException("Within Quobs method " + node.Method.Name + " is not supported");
         }
         Visit(node.Arguments[0]);
         return node;
      }
   }
}
