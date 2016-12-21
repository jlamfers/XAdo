namespace XAdo.SqlObjects.Search
{
   public class Paging
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; }
   }

   public static class PagingExtension
   {
      public static int GetSkip(this Paging self)
      {
         return (self.PageNumber - 1)*self.PageSize;
      }
   }
}
