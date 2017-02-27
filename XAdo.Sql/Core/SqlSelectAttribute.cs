using System;

namespace XAdo.Sql.Core
{
   public class SqlSelectAttribute : Attribute
   {
      public SqlSelectAttribute(string sql)
      {
         Sql = sql;
      }
      public string Sql { get; private set; }
   }
}
