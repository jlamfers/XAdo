using XAdo.Sql;

namespace XAdo.Model
{
   [SqlSelect("select id+,firstname!,middlename,lastname! from person.person")]
   public class Person
   {
      public int Id { get; set; }
      public string FirstName { get; set; }
      public string MiddelName { get; set; }
      public string LastName { get; set; }
   }
}
