using System;
using System.Collections;
using System.Linq.Expressions;
using XAdo.Quobs.Core;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface IReadSqlObject : ISqlObject, ICloneable
   {
      bool Any();
      int Count();
      IReadSqlObject Where(Expression expression);
      IReadSqlObject Union(IReadSqlObject sqlReadObject);
      IReadSqlObject OrderBy(bool keepOrder, bool descending, params Expression[] expressions);
      IReadSqlObject Distinct();
      IReadSqlObject Skip(int skip);
      IReadSqlObject Take(int take);
      IEnumerable FetchToEnumerable();
      IReadSqlObject Attach(ISqlConnection executer);

   }
}