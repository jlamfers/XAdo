using System;
using System.Runtime.Serialization;

namespace XAdo.Quobs
{
   [Serializable]
   public class SqlObjectsException : Exception
   {

      public SqlObjectsException()
      {
      }

      public SqlObjectsException(string message) : base(message)
      {
      }

      public SqlObjectsException(string message, Exception inner) : base(message, inner)
      {
      }

      protected SqlObjectsException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
      }
   }
}
