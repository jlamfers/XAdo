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
   public partial class SqlResource<TEntity> : SqlResource, ISqlResource<TEntity>
   {
      internal SqlResource(SqlResource other)
         : base(other)
      {
         
      }
      public SqlResource(IList<SqlPartial> partials, ISqlDialect dialect, IFilterParser urlParser)
         : base(partials, dialect, urlParser)
      {
      }

      public Expression<Func<IDataRecord, TEntity>> GetBinderExpression()
      {
         return GetBinderExpression<TEntity>();
      }

      public SqlResource<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression)
      {
         return Map<TEntity, TMap>(toExpression).ToGeneric<TMap>();
      }

      public Func<IDataRecord, TEntity> GetBinder()
      {
         return GetBinder<TEntity>();
      }

      public SqlPredicateCompiler.Result BuildSqlPredicate(Expression<Func<TEntity, bool>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return base.BuildSql(expression, arguments, parameterPrefix, noargs);
      }
      public SqlPredicateCompiler.Result BuildSql(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null, string parameterPrefix = "xado_", bool noargs = false)
      {
         return base.BuildSql(expression, arguments, parameterPrefix, noargs);
      }
      public string BuildSqlOrderBy(bool descending, params Expression<Func<TEntity, object>>[] columns)
      {
         return BuildSqlOrderBy(descending, columns.Cast<Expression>().ToArray());
      }


   }
}
