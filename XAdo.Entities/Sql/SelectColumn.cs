namespace XAdo.Quobs.Sql
{
   public class SelectColumn
   {
      public SelectColumn() { }

      public SelectColumn(string expression, string alias = null)
      {
         Expression = expression;
         Alias = alias;
      }
      public string Expression { get; set; }
      public string Alias { get; set; }

      public override string ToString()
      {
         return Expression + (Alias != null ? (" AS " + Alias) : "");
      }
   }
}