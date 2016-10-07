using System;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Method)]
   public class JoinMethodAttribute : Attribute
   {
      public string Expression { get; set; }
   }
}
