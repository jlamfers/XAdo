using System;

namespace XAdo.Sql
{
   public class SqlSelectAttribute : Attribute
   {

      public SqlSelectAttribute(string sqlSelect)
      {
         SqlSelect = sqlSelect;
      }

      public string SqlSelect { get; private set; }
   }
}