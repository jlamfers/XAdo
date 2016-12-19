using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
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
      private readonly Action<Expression, SqlBuilderContext> _parentMemberWriter;

      public SubQuery(ISqlFormatter formatter, IAliases aliases, Action<Expression,SqlBuilderContext> parentMemberWriter)
      {
         _formatter = formatter;
         _aliases = aliases;
         _parentMemberWriter = parentMemberWriter;
      }

      public ITableSqlObject<TTable> From<TTable>() where TTable : IDbTable
      {
         var sqlObject = new TableSqlObject<TTable>(_formatter);
         sqlObject.ParentMemberWriter = _parentMemberWriter;
         sqlObject.CastTo<IReadSqlObject>().Aliases = _aliases;
         return sqlObject;
      }
   }
}
