namespace XAdo.Quobs.Sql
{
   public class OrderColumn
   {
      public OrderColumn() { }

      public OrderColumn(string expression, bool descending = false)
      {
         Expression = expression;
         Descending = descending;
      }

      public string Expression { get; set; }
      public bool Descending { get; set; }

      public override string ToString()
      {
         return string.Format("{0}{1}", Expression, Descending ? " DESC" : "");
      }
   }
}