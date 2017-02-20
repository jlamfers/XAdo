using System;
using System.Runtime.Serialization;

namespace Sql.Parser
{
   [Serializable]
   public class SqlParserException : Exception
   {
      public SqlParserException()
      {
      }

      public SqlParserException(string message) : base(message)
      {
      }

      public SqlParserException(string message, Exception inner) : base(message, inner)
      {
      }

      protected SqlParserException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
         Expression = info.GetString("Expression");
         Index = info.GetInt32("Index");
      }

      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);
         info.AddValue("Expression", Expression);
         info.AddValue("Index", Index);
      }

      public SqlParserException(string expression, int index, string message)
         : base (message)
      {
         Expression = expression;
         Index = index;
      }

      public SqlParserException(string expression, int index, string message, Exception inner)
         : base(message,inner)
      {
         Expression = expression;
         Index = index;
      }

      public string Expression { get; private set; }
      public int Index { get; private set; }
   }
}
