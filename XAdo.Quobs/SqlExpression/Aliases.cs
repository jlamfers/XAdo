namespace XAdo.SqlObjects.SqlExpression
{
   public class Aliases : IAliases
   {
      private readonly IAliases _parent;

      public Aliases()
      {
         
      }
      public Aliases(IAliases parent)
      {
         _parent = parent;
      }

      public string Column(int index)
      {
         if (_parent != null)
         {
            return "c" + _parent.Column(index);
         }
         return "c" + index;
      }
      public string Table(int index)
      {
         if (_parent != null)
         {
            return "t" + _parent.Table(index);
         }
         return "t" + index;
      }
      public string TempTable(int index)
      {
         if (_parent != null)
         {
            return "x" + _parent.TempTable(index);
         }
         return "x" + index;
      }
      public string InParameter(int index)
      {
         if (_parent != null)
         {
            return "i" + _parent.InParameter(index);
         }
         return "in" + index + "_";
      }
      public string Parameter(int index)
      {
         if (_parent != null)
         {
            return "p" + _parent.Parameter(index);
         }
         return "p" + index;
      }
   }
}
