using System;
using System.Collections;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core
{
   public interface IQuob
   {
      IQuob Where(Expression expression);
      IQuob OrderBy(bool keepOrder, bool descending, params Expression[] expressions);
      int Count();
      bool Any();
      IQuob Distinct();
      IQuob Skip(int skip);
      IQuob Take(int take);
      IEnumerable ToEnumerable();
      IQuob Select(LambdaExpression expression);
      IQuob Attach(ISqlExecuter executer);
   }
}