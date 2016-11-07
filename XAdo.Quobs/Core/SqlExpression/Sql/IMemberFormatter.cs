using System;
using System.Reflection;

namespace XAdo.Quobs.Core.SqlExpression.Sql
{
   public interface IMemberFormatter
   {
      string FormatColumn(ISqlFormatter formatter, MemberInfo member);
      string FormatTable(ISqlFormatter formatter, Type type);
   }
}
