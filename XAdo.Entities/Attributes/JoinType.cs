using System;

namespace XAdo.Quobs.Attributes
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
      public static string ToSqlJoinExpression(this JoinType self, string expresion)
      {
         switch (self)
         {
            case JoinType.Inner:
               return "INNER " + expresion;
            case JoinType.Left:
               return "LEFT OUTER " + expresion;
            case JoinType.Right:
               return "RIGHT OUTER " + expresion;
            case JoinType.Full:
               return "FULL OUTER " + expresion;
            default:
               throw new ArgumentOutOfRangeException("self");
         }
      }
   }
}
