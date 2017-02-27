using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.Parser.Partials;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.Core
{
   //immutable object
   public partial class QueryBuilder<TEntity> : QueryBuilder, IQueryBuilder<TEntity>
   {
      internal QueryBuilder(QueryBuilder other)
         : base(other)
      {
         
      }
      public QueryBuilder(IList<SqlPartial> partials, ISqlDialect dialect, IUrlExpressionParser urlParser)
         : base(partials, dialect, urlParser)
      {
      }

      public Expression<Func<IDataRecord, TEntity>> GetBinderExpression()
      {
         return GetBinderExpression<TEntity>();
      }

      public QueryBuilder<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression)
      {
         return Map<TEntity, TMap>(toExpression).ToGeneric<TMap>();
      }

      public Func<IDataRecord, TEntity> GetBinder()
      {
         return GetBinder<TEntity>();
      }

      public SqlGenerator.Result BuildSqlByExpression(Expression<Func<TEntity, bool>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return base.BuildSqlByExpression(expression, arguments, parameterPrefix, noargs);
      }
      public SqlGenerator.Result BuildSqlByExpression(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return base.BuildSqlByExpression(expression, arguments, parameterPrefix, noargs);
      }
      public string GetSqlOrderBy(bool descending, params Expression<Func<TEntity, object>>[] columns)
      {
         return GetSqlOrderBy(descending, columns.Cast<Expression>().ToArray());
      }


   }
}
