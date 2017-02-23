using System;
using System.Runtime.Serialization;

namespace XAdo.Sql.Core.Common
{
   [Serializable]
   public class XAdoSqlException : Exception
   {

      public XAdoSqlException()
      {
      }

      public XAdoSqlException(string message) : base(message)
      {
      }

      public XAdoSqlException(string message, Exception inner) : base(message, inner)
      {
      }

      protected XAdoSqlException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
      }
   }
}
