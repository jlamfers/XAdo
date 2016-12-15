using System;
using XAdo.Quobs.Dialects.Core;

namespace XAdo.Quobs.Dialects.SqlServer
{
   public class SqlServerFormatter : SqlFormatter
   {

      private static readonly DateTime MinDate = new DateTime(1753, 1, 1);

      public SqlServerFormatter()
         : this(new SqlServerDialect())
      {
         
      }

      protected SqlServerFormatter(ISqlDialect dialect) : base(dialect)
      {
      }

      public override object NormalizeValue(object value)
      {
         return value == null || Type.GetTypeCode(value.GetType()) != TypeCode.DateTime
                 ? value
                 : ((DateTime)value == DateTime.MinValue ? MinDate : value);
      }
   }
}
