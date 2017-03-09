using System.Collections.Generic;

namespace XAdo.Quobs.Core.Sorting
{
   public class TopologicalNode<T>
   {
      public T Item { get; set; }
      public List<T> DependsOn { get; set; }
   }
}