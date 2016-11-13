namespace XAdo.Quobs.Core
{
   internal static class Aliases
   {
      public static string Column(int index)
      {
         return "__c_" + index;
      }
      public static string Table(int index)
      {
         return "__t_" + index;
      }
      public static string TempTable(int index)
      {
         return "__tt_" + index;
      }
   }
}
