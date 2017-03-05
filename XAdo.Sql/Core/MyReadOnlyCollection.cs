using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace XAdo.Quobs.Core.Common
{
   public class MyReadOnlyCollection<T> : IReadOnlyCollection<T>, ICollection<T>
   {
      private readonly ICollection<T> _inner;

      public MyReadOnlyCollection(ICollection<T> other)
      {
         _inner = other;
      }

      public IEnumerator<T> GetEnumerator()
      {
         return _inner.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return ((IEnumerable)_inner).GetEnumerator();
      }

      public void Add(T item)
      {
         throw new ReadOnlyException();
      }

      public void Clear()
      {
         throw new ReadOnlyException();
      }

      public bool Contains(T item)
      {
         return _inner.Contains(item);
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         _inner.CopyTo(array, arrayIndex);
      }

      public bool Remove(T item)
      {
         throw new ReadOnlyException();
      }

      public int Count
      {
         get { return _inner.Count; }
      }

      public bool IsReadOnly
      {
         get { return true; }
      }
   }
}