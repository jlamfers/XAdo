using System;
using System.Reflection;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core
{
   public class MemberFormatter : IMemberFormatter
   {
      public string FormatColumn(ISqlFormatter formatter, MemberInfo member)
      {
         return formatter.FormatColumn(member.GetColumnDescriptor());
      }

      public string FormatTable(ISqlFormatter formatter, Type type)
      {
         return formatter.FormatTable(type.GetTableDescriptor());
      }
   }
}
