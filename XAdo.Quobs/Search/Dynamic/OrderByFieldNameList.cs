using System.Collections;
using System.Collections.Generic;

namespace XHour.Contract.Search.Dynamic
{
   public class OrderByFieldNameList : IEnumerable<OrderByFieldName>
   {
      private readonly List<OrderByFieldName> 
         _orderByFields = new List<OrderByFieldName>();

      public OrderByFieldNameList Add(string field, bool descending = false)
      {
         _orderByFields.Add(new OrderByFieldName{FieldName = field, Descending = descending});
         return this;
      }

      public IEnumerator<OrderByFieldName> GetEnumerator()
      {
         return _orderByFields.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}
