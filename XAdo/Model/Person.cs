using System;
using XAdo.Sql.Core;

namespace XAdo.Model
{
   public class SqlSelectAttribute : Attribute
   {
      public SqlSelectAttribute(string sqlSelect)
      {
         SqlSelect = sqlSelect;
      }
      public string SqlSelect { get; private set; }
   }

   [SqlSelect("select id+,firstname!,middlename,lastname! from person.person")]
   public class Person
   {
      public int Id { get; set; }
      public string FirstName { get; set; }
      public string MiddelName { get; set; }
      public string LastName { get; set; }
   }
}
