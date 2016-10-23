using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XAdo.Quobs.Sql.Formatter
{
   public interface ISqlFormatter
   {
      string FormatParameterName(string parameterName);
      string FormatColumn(MemberInfo member);
      string FormatTableName(Type entityType);
      string ConcatenateSqlStatements(IEnumerable<string> statements);
      ISqlFormatter FormatSqlMethod(string methodName, TextWriter writer, params Action<TextWriter>[] arguments);
      ISqlFormatter FormatSqlSelect(TextWriter writer, ISqlSelectMeta meta);
      ISqlFormatter FormatSqlSelectPaged(TextWriter writer, ISqlSelectMeta meta, string parNameSkip, string parNameTake);

      ISqlFormatter FormatSqlSelectCount(TextWriter writer, ISqlSelectMeta meta);

      string DelimitIdentifier(params string[] nameParts);
   }
}
