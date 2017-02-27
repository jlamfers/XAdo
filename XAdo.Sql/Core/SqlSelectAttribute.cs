using System;

namespace XAdo.Quobs.Core
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
