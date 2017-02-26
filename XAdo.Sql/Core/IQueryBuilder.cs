using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using XAdo.Core.Interface;
using XAdo.Sql.Core.Linq;
using XAdo.Sql.Core.Parser.Partials;

namespace XAdo.Sql.Core
{
   public interface IQueryBuilder
   {
      QueryBuilder AsCountQuery();
      void Format(TextWriter w, object args);
      string Format(object args);
      IList<SqlPartial> Partials { get; }
      IDictionary<string, MetaColumnPartial> MappedColumns { get; }
      WithPartial With { get; }
      SelectPartial Select { get; }
      TablePartial Table { get; }
      IList<JoinPartial> Joins { get; }
      WherePartial Where { get; }
      GroupByPartial GroupBy { get; }
      HavingPartial Having { get; }
      HavingPartial OrderBy { get; }
      Expression<Func<IDataRecord, T>> GetBinderExpression<T>();
      Expression GetBinderExpression(Type entityType);
      QueryBuilder Map<TEntity, TMap>(Expression<Func<TEntity, TMap>> toExpression);
      QueryBuilder Map(LambdaExpression toExpression);
      QueryBuilder Map(string selectExpression, Type mappedType);
      SqlGenerator.Result BuildSqlFromExpression(Expression expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      Func<IDataRecord, T> GetBinder<T>();
      Delegate GetBinder(Type entityType);
      Func<IDataRecord, object> GetBinder(IAdoSession session);
      Type GetBinderType(IAdoSession session);
      QueryBuilder<TEntity> ToGeneric<TEntity>();
      QueryBuilder CreateMap(IList<SqlPartial> partials);
   }
}