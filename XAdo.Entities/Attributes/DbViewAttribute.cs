using System;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Class)]
   public class DbViewAttribute : Attribute
   {
      public DbViewAttribute()
      {
         IsReadOnly = true;
      }

      public bool IsReadOnly { get; set; }
   }
}