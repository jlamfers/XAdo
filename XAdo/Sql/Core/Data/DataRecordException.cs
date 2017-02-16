using System;
using System.Runtime.Serialization;

namespace XAdo.Sql.Core
{
   [Serializable]
   public class DataRecordException : Exception
   {
      public DataRecordException()
      {
         Index = -1;
      }
      public DataRecordException(int index)
      {
         Index = index;
      }
      public DataRecordException(int index, string message) : base(message)
      {
         Index = index;
      }
      public DataRecordException(int index, string message, Exception inner) : base(message, inner)
      {
         Index = index;
      }
      public DataRecordException(int index, Exception inner)
         : this(index, "error at data reader: value is null at index "+index , inner)
      {
         Index = index;
      }

      protected DataRecordException(
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