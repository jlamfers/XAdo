using System;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
   public class JoinMethodAttribute : Attribute
   {
      public string Expression { get; set; }
   }
}
