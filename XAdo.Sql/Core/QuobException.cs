using System;
using System.Runtime.Serialization;
using XAdo.Core;

namespace XAdo.Quobs
{
   [Serializable]
   public class QuobException : XAdoException
   {
      public QuobException()
      {
      }

      public QuobException(string message) : base(message)
      {
      }

      public QuobException(string message, Exception inner) : base(message, inner)
      {
      }

      protected QuobException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
      }
   }
}
