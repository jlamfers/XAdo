using System;
using System.Runtime.Serialization;

namespace XAdo.Sql.Core
{
   [Serializable]
   public class DbReaderException : Exception
   {
      public DbReaderException()
      {
         Index = -1;
      }
      public DbReaderException(int index)
      {
         Index = index;
      }
      public DbReaderException(int index, string message) : base(message)
      {
         Index = index;
      }
      public DbReaderException(int index, string message, Exception inner) : base(message, inner)
      {
         Index = index;
      }
      public DbReaderException(int index, Exception inner)
         : this(index, "error at dbreader: value is null at index "+index , inner)
      {
         Index = index;
      }

      protected DbReaderException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
         Index = info.GetInt32("Index");
      }

      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("Index",Index);
      }

      public int Index { get; private set; }
   }
}