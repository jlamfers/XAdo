namespace XAdo.SqlObjects.SqlExpression
{
   public interface IAliases
   {
      string Column(int index);
      string Table(int index);
      string TempTable(int index);
      string InParameter(int index);
      string Parameter(int index);
   }
}