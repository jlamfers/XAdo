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
   public interface ISqlResource
   {
      SqlResource AsCountQuery();

      void Format(TextWriter w, object templateArgs);
      string Format(object templateArgs);

      IList<SqlPartial> Partials { get; }
      IDictionary<string, ColumnPartial> MappedColumns { get; }
      IList<TablePartial> Tables { get; }


      WithPartial With { get; }
      SelectPartial Select { get; }
      TablePartial Table { get; }
      IList<JoinPartial> Joins { get; }
      WherePartial Where { get; }
      GroupByPartial GroupBy { get; }
      HavingPartial Having { get; }
      HavingPartial OrderBy { get; }

      ISqlDialect Dialect { get; }

      SqlResource Map<TEntity, TMap>(Expression<Func<TEntity, TMap>> toExpression);
      SqlResource Map(LambdaExpression toExpression);
      SqlResource Map(string selectExpression, Type mappedType);
      SqlResource CreateMap(IList<SqlPartial> partials); //todo: add type, session????

      SqlPredicateCompiler.Result BuildSql(Expression expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      SqlPredicateCompiler.Result BuildSqlPredicate(string filterExpression, Type mappedType, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      string BuildSqlOrderBy(string orderExpression, Type mappedType);

      Expression<Func<IDataRecord, T>> GetBinderExpression<T>();
      Expression GetBinderExpression(Type entityType);
      Func<IDataRecord, T> GetBinder<T>();
      Delegate GetBinder(Type entityType);
      Func<IDataRecord, object> GetBinder(IAdoSession session);
      Type GetEntityType(IAdoSession session);

      SqlResource<TEntity> ToGeneric<TEntity>();

      string BuildSqlOrderBy(bool descending, params Expression[] columns);
   }

   public interface ISqlResource<TEntity> : ISqlResource
   {
      Expression<Func<IDataRecord, TEntity>> GetBinderExpression();
      Func<IDataRecord, TEntity> GetBinder();

      SqlResource<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression);

      SqlPredicateCompiler.Result BuildSqlPredicate(Expression<Func<TEntity, bool>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      SqlPredicateCompiler.Result BuildSql(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      string BuildSqlOrderBy(bool descending, params Expression<Func<TEntity, object>>[] columns);
   }
}