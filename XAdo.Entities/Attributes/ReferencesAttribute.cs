using System;
using XAdo.Quobs.Schema;

namespace XAdo.Quobs.Attributes
{
   public class ReferencesAttribute : Attribute
   {
      public Type Type { get; set; }
      public string Member { get; set; }
      public string Column { get; set; }
      public string Constraint { get; set; }
   }
}