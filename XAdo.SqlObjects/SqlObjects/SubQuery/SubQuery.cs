using System;
using System.Linq.Expressions;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects.SubQuery
{
   public class SubQuery : ISubQuery
   {
      private readonly ISqlFormatter _formatter;
      private readonly IAliases _aliases;
      private readonly Action<Expression, SqlBuilderContext> _callbackWriter;

      public SubQuery(ISqlFormatter formatter, IAliases aliases, Action<Expression,SqlBuilderContext> callbackWriter)
      {
         _formatter = formatter;
         _aliases = aliases;
         _callbackWriter = callbackWriter;
      }

      public QuerySqlObject<TTable> From<TTable>() where TTable : IDbTable
      {
         var sqlObject = new QuerySqlObject<TTable>(_formatter) {CallbackWriter = _callbackWriter};
         sqlObject.CastTo<IReadSqlObject>().Aliases = _aliases;
         return sqlObject;
      }
   }
}
