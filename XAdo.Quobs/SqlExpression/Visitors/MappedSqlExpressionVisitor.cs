using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace XAdo.SqlObjects.SqlExpression.Visitors
{
   public class MappedSqlExpressionVisitor : SqlExpressionVisitor
   {
      private readonly IDictionary<string, string> _sqlMap;

      public MappedSqlExpressionVisitor(IDictionary<string, string> sqlMap)
      {
         _sqlMap = sqlMap;
      }

      protected override Expression VisitMember(MemberExpression exp)
      {
         string sql;
         if (_sqlMap.TryGetValue(exp.Member.Name, out sql))
         {
            Context.Writer.Write(sql);
            return exp;
         }
         return base.VisitMember(exp);
      }
   }
}
