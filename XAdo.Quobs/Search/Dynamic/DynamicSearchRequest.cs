namespace XAdo.SqlObjects.Search.Dynamic
{
   public class DynamicSearchRequest
   {
      public Filter Filter { get; set; }
      public Paging Paging { get; set; }
      public OrderByFieldNameList OrderByFields { get; set; }
   }
}
