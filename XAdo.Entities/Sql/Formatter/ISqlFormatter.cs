using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XAdo.Quobs.Sql.Formatter
{
   public interface ISqlFormatter
   {
      ISqlFormatter FormatSqlMethod(string methodName, TextWriter writer, params Action<TextWriter>[] arguments);
      //ISqlFormatter FormatSqlSelectPaged(TextWriter writer, ISqlSelectMeta meta, string parNameSkip, string parNameTake);
      string ParameterPrefix { get; }
      string StatementSeperator { get; }
      string IdentifierDelimiterLeft { get; }
      string IdentifierDelimiterRight { get; }
   }
}
