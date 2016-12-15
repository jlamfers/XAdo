namespace XAdo.SqlObjects.SqlExpression
{
   internal static class Aliases
   {
      public static string Column(int index)
      {
         return "c" + index;
      }
      public static string Table(int index)
      {
         return "t" + index;
      }
      public static string TempTable(int index)
      {
         return "tt" + index;
      }
   }
}
