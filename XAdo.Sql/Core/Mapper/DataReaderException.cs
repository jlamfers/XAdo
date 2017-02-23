using System;
using System.Runtime.Serialization;

namespace XAdo.Sql.Core.Mapper
{
   [Serializable]
   public class DataReaderException : Exception
   {
      public DataReaderException()
      {
         Index = -1;
      }
      public DataReaderException(int index)
      {
         Index = index;
      }
      public DataReaderException(int index, string message) : base(message)
      {
         Index = index;
      }
      public DataReaderException(int index, string message, Exception inner) : base(message, inner)
      {
         Index = index;
      }
      public DataReaderException(int index, Exception inner)
         : this(index, "error at data reader: value is null at index "+index , inner)
      {
         Index = index;
      }

      protected DataReaderException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
         Index = info.GetInt32("Index");
      }

      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info,context);
         info.AddValue("Index",Index);
      }

      public int Index { get; private set; }
   }
}