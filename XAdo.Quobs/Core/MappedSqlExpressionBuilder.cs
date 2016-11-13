using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.Core
{
   public class MappedSqlExpressionBuilder : SqlExpressionBuilder
   {
      private readonly IDictionary<MemberInfo, string> _sqlMap;

      public MappedSqlExpressionBuilder(IDictionary<MemberInfo, string> sqlMap)
      {
         _sqlMap = sqlMap;
      }

      protected override Expression VisitMember(MemberExpression exp)
      {
         string sql;
         if (_sqlMap.TryGetValue(exp.Member, out sql))
         {
            Context.Writer.Write(sql);
            return exp;
         }
         return base.VisitMember(exp);
      }
   }
}
