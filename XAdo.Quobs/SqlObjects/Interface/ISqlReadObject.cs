using System;
using System.Collections;
using System.Linq.Expressions;
using XAdo.Quobs.Core;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ISqlReadObject : ISqlObject, ICloneable
   {
      bool Any();
      int Count();
      ISqlReadObject Where(Expression expression);
      ISqlReadObject Union(ISqlReadObject sqlReadObject);
      ISqlReadObject OrderBy(bool keepOrder, bool descending, params Expression[] expressions);
      ISqlReadObject Distinct();
      ISqlReadObject Skip(int skip);
      ISqlReadObject Take(int take);
      IEnumerable FetchToEnumerable();
      ISqlReadObject Attach(ISqlConnection executer);

   }
}