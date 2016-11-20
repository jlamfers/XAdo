using System;

namespace XAdo.Quobs.Dialect
{
   public class SqlServerFormatter : SqlFormatter
   {

      private static readonly DateTime MinDate = new DateTime(1753, 1, 1);

      public SqlServerFormatter() : base(new SqlServerDialect())
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
