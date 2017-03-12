using System.Collections.Generic;

namespace XAdo.DbSchema
{
   public class TopologicalSortNode<T>
   {
      public T Item { get; set; }
      public IList<T> DependsOn { get; set; }
   }
}