using System;
using System.IO;

namespace XAdo.Quobs.Sql.Formatter
{
   public class SqlServerFormatter : SqlFormatter
   {
      public SqlServerFormatter()
      {
         ParameterPrefix = "@";
         StatementSeperator = ";";
         IdentifierDelimiterLeft = "[";
         IdentifierDelimiterRight = "]";

      }

      public override ISqlFormatter FormatSqlMethod(string methodName, TextWriter writer, params Action<TextWriter>[] arguments)
      {
         methodName = methodName.ToUpper();

         if (methodName != "CONCAT") 
            return base.FormatSqlMethod(methodName, writer, arguments);

         var plus = "";
         foreach (var arg in arguments)
         {
            writer.Write(plus);
            arg(writer);
            plus = " + ";
         }
         return this;
      }

   }
}