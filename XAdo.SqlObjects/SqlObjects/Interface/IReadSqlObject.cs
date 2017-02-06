using System;
using System.Collections;
using System.Linq.Expressions;
using XAdo.SqlObjects.SqlExpression;

namespace XAdo.SqlObjects.SqlObjects.Interface
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
      IReadSqlObject Attach(ISqlConnection executer);
      IAliases Aliases { get; set; }

   }
}