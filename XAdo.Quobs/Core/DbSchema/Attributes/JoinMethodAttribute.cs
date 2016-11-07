using System;

namespace XAdo.Quobs.Core.DbSchema.Attributes
{
   [AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
   public class JoinMethodAttribute : Attribute
   {
      /// <summary>
      /// Join expression without join type (no INNER, LEFT, RIGHT etc...)
      /// </summary>
      public string Expression { get; set; }

      /// <summary>
      /// If true, then this join represents a *-N cardinality, else *-1
      /// </summary>
      public bool NChilds { get; set; }

      public Type LeftTableType { get; set; }

      public Type RightTableType { get; set; }

   }
}
