using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Parser.Partials;
using XAdo.Quobs.Dialects;

namespace XAdo.Quobs.Interface
{
   public interface ISqlResource
   {
      ISqlResource AsCountQuery();

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

      ISqlResource Map(LambdaExpression toExpression);
      ISqlResource Map(string selectExpression, Type mappedType);
      ISqlResource CreateMap(IList<SqlPartial> partials); //todo: add type, session????

      SqlGeneratorResult BuildSql(Expression expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      SqlGeneratorResult BuildSqlPredicate(string filterExpression, Type mappedType, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      string BuildSqlOrderBy(string orderExpression, Type mappedType);

      Expression<Func<IDataRecord, T>> GetBinderExpression<T>();
      Expression GetBinderExpression(Type entityType);
      Func<IDataRecord, T> GetBinder<T>();
      Delegate GetBinder(Type entityType);
      Func<IDataRecord, object> GetBinder(IXAdoDbSession session);
      Type GetEntityType(IXAdoDbSession session);

      ISqlResource<TEntity> ToGeneric<TEntity>();

      string BuildSqlOrderBy(bool descending, params Expression[] columns);
   }

   public interface ISqlResource<TEntity> : ISqlResource
   {
      Expression<Func<IDataRecord, TEntity>> GetBinderExpression();
      Func<IDataRecord, TEntity> GetBinder();

      ISqlResource<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression);

      SqlGeneratorResult BuildSqlPredicate(Expression<Func<TEntity, bool>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      SqlGeneratorResult BuildSql(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false);
      string BuildSqlOrderBy(bool descending, params Expression<Func<TEntity, object>>[] columns);
   }
}