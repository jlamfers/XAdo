using System;

namespace XAdo.Entities.Attributes
{
   public class DbNameAttribute : Attribute
   {

      public DbNameAttribute(string name)
      {
         Name = name;
      }

      public string Name { get; private set; }
   }
}
