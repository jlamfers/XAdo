using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XAdo.Entities.Sql.Formatter
{
   public interface ISqlFormatter
   {
      string FormatParameterName(string parameterName);
      string FormatColumnName(MemberInfo member);
      string FormatTableName(Type entityType);
      string ConcatenateSqlStatements(IEnumerable<string> statements);
      string DelimitIdentifier(string qualifiedName);
      ISqlFormatter FormatSqlMethod(string methodName, TextWriter writer, params Action<TextWriter>[] arguments);
      ISqlFormatter FormatSqlSelect(TextWriter writer, ISqlSelectMeta meta);

   }
}
