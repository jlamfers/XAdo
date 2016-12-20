using System.Collections.Generic;

namespace XHour.Contract.Search.Dynamic
{
   public class Filter
   {
      public Filter()
      {
         Criteria = new List<Criterium>();
      }

      public List<Criterium> Criteria { get; internal set; }
   }
}