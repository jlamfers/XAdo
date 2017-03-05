using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Impl
{
   //immutable object
   public partial class SqlResource<TEntity> : SqlResourceImpl, ISqlResource<TEntity>
   {
      
      internal SqlResource(SqlResourceImpl other)
         : base(other)
      {
         
      }
      public SqlResource(IList<SqlPartial> partials, ISqlDialect dialect, IFilterParser urlParser, ISqlPredicateGenerator sqlPredicateGenerator)
         : base(partials, dialect, urlParser, sqlPredicateGenerator)
      {
          }

      public Expression<Func<IDataRecord, TEntity>> GetBinderExpression()
      {
         return GetBinderExpression<TEntity>();
      }

      public ISqlResource<TMap> Map<TMap>(Expression<Func<TEntity, TMap>> toExpression)
      {
         return Map((LambdaExpression)toExpression).ToGeneric<TMap>();
      }

      public Func<IDataRecord, TEntity> GetBinder()
      {
         return GetBinder<TEntity>();
      }

      public SqlGeneratorResult BuildSqlPredicate(Expression<Func<TEntity, bool>> expression, IDictionary<string, object> arguments = null)
      {
         return base.BuildSql(expression, arguments);
      }
      public SqlGeneratorResult BuildSql(Expression<Func<TEntity, object>> expression, IDictionary<string, object> arguments = null)
      {
         return base.BuildSql(expression, arguments);
      }
      public string BuildSqlOrderBy(bool descending, params Expression<Func<TEntity, object>>[] columns)
      {
         return BuildSqlOrderBy(descending, columns.Cast<Expression>().ToArray());
      }


   }
}
