using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;

namespace XAdo.Quobs.Linq
{
   class WrappedQuob<T> : IQuob
   {
      private readonly IEnumerable<T> _enumerable;

      public WrappedQuob(IEnumerable<T> enumerable)
      {
         _enumerable = enumerable;
      }

      public IQuob Where(Expression expression)
      {
         var exp = (Expression<Func<T, bool>>) expression;
         return new WrappedQuob<T>(_enumerable.Where(exp.Compile()));
      }

      public IQuob OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         var e = ToEnumerable();
         foreach (var ex in expressions)
         {
            var lambda = ex.Unquote().CastTo<LambdaExpression>();
            var type = lambda.Body.Type;
            var helper = typeof (OrderByHelper<>).MakeGenericType(typeof (T), type).CreateInstance<IOrderByHelper>();
            e = helper.OrderBy(e, keepOrder, @descending, lambda);
         }
         return new WrappedQuob<T>(e.CastTo<IEnumerable<T>>());
      }

      private interface IOrderByHelper
      {
         IEnumerable<T> OrderBy(IEnumerable enumerable, bool keepOrder, bool @descending, Expression expression);
      }

      private class OrderByHelper<TKey> : IOrderByHelper
      {
         public IEnumerable<T> OrderBy(IEnumerable enumerable, bool keepOrder, bool @descending, Expression expression)
         {
            var e = (IEnumerable<T>) enumerable;
            return keepOrder
               ? (!@descending
                  ? e.CastTo<IOrderedEnumerable<T>>().ThenBy(expression.CastTo<Expression<Func<T, TKey>>>().Compile())
                  : e.CastTo<IOrderedEnumerable<T>>()
                     .ThenByDescending(expression.CastTo<Expression<Func<T, TKey>>>().Compile()))
               : (!@descending
                  ? e.OrderBy(expression.CastTo<Expression<Func<T, TKey>>>().Compile())
                  : e.OrderByDescending(expression.CastTo<Expression<Func<T, TKey>>>().Compile()));
         }
      }

      public int Count()
      {
         return _enumerable.Count();
      }

      public bool Any()
      {
         return _enumerable.Any();
      }

      public IQuob Distinct()
      {
         return new WrappedQuob<T>(_enumerable.Distinct());
      }

      public IQuob Skip(int skip)
      {
         return new WrappedQuob<T>(_enumerable.Skip(skip));
      }

      public IQuob Take(int take)
      {
         return new WrappedQuob<T>(_enumerable.Take(take));
      }

      public IEnumerable ToEnumerable()
      {
         return _enumerable;
      }

      public IQuob Select(LambdaExpression expression)
      {
         var t = typeof(MapExpressionHelper<>);
         t = t.MakeGenericType(typeof(T), expression.Body.Type);
         var helper = t.CreateInstance<IMapExpressionHelper>();
         return helper.Select(this, expression);
      }

      public IQuob Connect(ISqlExecuter executer)
      {
         throw new NotImplementedException();
      }

      private interface IMapExpressionHelper
      {
         IQuob Select(IQuob quob, LambdaExpression expression);
      }
      private class MapExpressionHelper<TMapped> : IMapExpressionHelper
      {
         public IQuob Select(IQuob quob, LambdaExpression expression)
         {
            var f = (Func<T, TMapped>)expression.Compile();

            //todo: interface IQuob<T> ??
            if (quob is MappedQuob<T>)
            {
               var q = (MappedQuob<T>) quob;
               return new WrappedQuob<TMapped>(q.ToList().Select(f));
            }
            var q2 = (WrappedQuob<T>)quob;
            return new WrappedQuob<TMapped>(q2.ToEnumerable().CastTo<IEnumerable<T>>().ToList().Select(f));
         }
      }

   }
}
