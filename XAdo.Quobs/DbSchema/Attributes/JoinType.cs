using System;

namespace XAdo.Quobs.DbSchema.Attributes
{
   public enum JoinType
   {
      Inner,
      Left,
      Right,
      Full
   }

   public static class JoinTypeExtension
   {
      public static string ToJoinTypeString(this JoinType self)
      {
         switch (self)
         {
            case JoinType.Inner:
               return "INNER";
            case JoinType.Left:
               return "LEFT OUTER";
            case JoinType.Right:
               return "RIGHT OUTER";
            case JoinType.Full:
               return "FULL OUTER";
            default:
               throw new ArgumentOutOfRangeException("self");
         }
      }
   }
}
