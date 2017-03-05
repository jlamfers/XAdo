using System;
using System.Runtime.Serialization;
using System.Text;

namespace XAdo.Quobs.Core.Parser
{
   [Serializable]
   public class SqlParserException : QuobException
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
         : base(MakeMessage(expression, index, message))
      {
         Expression = expression;
         Index = index;
      }

      public SqlParserException(string expression, int index, string message, Exception inner)
         : base(MakeMessage(expression,index,message),inner)
      {
         Expression = expression;
         Index = index;
      }

      public string Expression { get; private set; }
      public int Index { get; private set; }

      private static string MakeMessage(string expression, int index, string message)
      {
         var markedExpression = new StringBuilder();
         var i = -1;
         var line = 1;
         var col = 0;
         var markeradded = false;
         foreach (var ch in expression)
         {
            col++;
            i++;
            if (ch == '\n' || ch == '\r')
            {
               col = 0;
               if (ch == '\n')
               {
                  line++;
               }
               if (i > index)
               {
                  break;
               }
               markedExpression.Length = 0;
               continue;
            }
            if (i == index)
            {
               markeradded = true;
               markedExpression.Append("=>");
            }
            markedExpression.Append(ch);
            if (i == index)
            {
               markedExpression.Append("<=");
            }

         }
         if (!markeradded)
         {
            markedExpression.Append("=>?<=");
         }
         return message + Environment.NewLine + "at line {0}, column {1}:".FormatWith(line,col) + Environment.NewLine + markedExpression;

      }
   }
}
