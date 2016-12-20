namespace XHour.Contract.Search.Typed
{
   public class OrderByFieldExpression
   {
      public System.Linq.Expressions.Expression Field { get; set; }
      public bool Descending { get; set; }
   }
}