using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Parser.Partials;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.Core
{
   public interface IQueryBuilder<TEntity> : IQueryBuilder
   {
      Expression<Func<IDataRecord, TEntity>> GetBinderExpression();
      QueryBuilder<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression);
      Func<IDataRecord, TEntity> GetBinder();
      SqlGenerator.Result BuildSqlByExpression(Expression<Func<TEntity, bool>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      SqlGenerator.Result BuildSqlByExpression(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      string GetSqlOrderBy(bool descending, params Expression<Func<TEntity, object>>[] columns);
   }

   public interface IQueryBuilder
   {
      QueryBuilder AsCountQuery();
      void Format(TextWriter w, object templateArgs);
      string Format(object templateArgs);
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
      ISqlDialect Dialect { get; }
      IList<TablePartial> Tables { get; }

      Expression<Func<IDataRecord, T>> GetBinderExpression<T>();
      Expression GetBinderExpression(Type entityType);
      QueryBuilder Map<TEntity, TMap>(Expression<Func<TEntity, TMap>> toExpression);
      QueryBuilder Map(LambdaExpression toExpression);
      QueryBuilder Map(string selectExpression, Type mappedType);
      SqlGenerator.Result BuildSqlByExpression(Expression expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      SqlGenerator.Result BuildSqlPredicate(string filterExpression, Type mappedType, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      string BuildSqlOrderBy(string orderExpression, Type mappedType);
      Func<IDataRecord, T> GetBinder<T>();
      Delegate GetBinder(Type entityType);
      Func<IDataRecord, object> GetBinder(IAdoSession session);
      Type GetBinderType(IAdoSession session);
      QueryBuilder<TEntity> ToGeneric<TEntity>();
      QueryBuilder CreateMap(IList<SqlPartial> partials); //todo: add type, session????
      string BuildSqlOrderBy(bool descending, params Expression[] columns);
   }
}