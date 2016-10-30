using System;
using System.IO;
using XAdo.Quobs.Descriptor;

namespace XAdo.Quobs.Sql.Formatter
{
   public interface ISqlFormatter
   {
      ISqlFormatter FormatSqlMethod(string methodName, TextWriter writer, params Action<TextWriter>[] arguments);
      ISqlFormatter FormatPageQuery(TextWriter writer, QueryDescriptor descriptor);
      string ParameterPrefix { get; }
      string StatementSeperator { get; }
      string IdentifierDelimiterLeft { get; }
      string IdentifierDelimiterRight { get; }
   }
}
