using System;

namespace XAdo.Quobs.Core.DbSchema.Attributes
{
   [AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
   public class JoinMethodAttribute : Attribute
   {
      /// <summary>
      /// If true, then this join represents a this=>N cardinality, else this=>1
      /// </summary>
      public bool Reversed { get; set; }

      public string RelationshipName { get; set; }
   }
}
