using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XAdo.Entities.Attributes;

namespace XAdo.Entities.Sql.Formatter
{
   public class SqlFormatter : ISqlFormatter
   {
      public const string LiteralEscape = "!";

      protected string ParameterPrefix { get; set; }
      protected string StatementSeperator { get; set; }
      protected string IdentifierDelimiterLeft { get; set; }
      protected string IdentifierDelimiterRight { get; set; }

      public SqlFormatter()
      {
         ParameterPrefix = "@";
         StatementSeperator = ";";
         IdentifierDelimiterLeft = "\"";
         IdentifierDelimiterRight = "\"";

      }

      public virtual string FormatParameterName(string parameterName)
      {
         if (parameterName == null) throw new ArgumentNullException("parameterName");
         return ParameterPrefix + parameterName;
      }
      public virtual string FormatColumnName(MemberInfo member)
      {
         if (member == null) throw new ArgumentNullException("member");
         var a = member.GetCustomAttribute<DbNameAttribute>();
         return a != null
            ? (a.Name.StartsWith(LiteralEscape) ? a.Name : DelimitIdentifier(a.Name))
            : DelimitIdentifier(member.Name);
      }
      public virtual string FormatTableName(Type entityType)
      {
         if (entityType == null) throw new ArgumentNullException("entityType");
         return FormatColumnName(entityType);
      }

      public virtual string ConcatenateSqlStatements(IEnumerable<string> statements)
      {
         return string.Join(StatementSeperator + "\r\n", statements.ToArray());
      }

      public virtual ISqlFormatter FormatSqlMethod(string methodName, TextWriter writer, params Action<TextWriter>[] arguments)
      {
         methodName = methodName.ToUpper();
         // default handling: format method as methodName(arg1,arg2,arg3, etc...)
         var comma = "";
         writer.Write(methodName);
         writer.Write("(");
         foreach (var arg in arguments)
         {
            writer.Write(comma);
            arg(writer);
            comma = ", ";
         }
         writer.Write(")");
         return this;
      }
      public virtual ISqlFormatter FormatSqlSelect(TextWriter writer, ISqlSelectMeta meta)
      {
         var w = writer;

         if (!meta.SelectColumns.Any())
         {
            w.WriteLine("SELECT *");
         }
         else
         {
            w.WriteLine("SELECT");
            w.WriteLine("   " + string.Join(",\r\n   ", meta.SelectColumns.ToArray()));
         }
         w.WriteLine("FROM {0}", meta.TableName);
         if (meta.WhereClausePredicates.Any())
         {
            w.WriteLine("WHERE");
            w.WriteLine("   " + string.Join("\r\n   AND ", meta.WhereClausePredicates.Select(s => "(" + s + ")").ToArray()));
         }
         if (meta.GroupByColumns.Any())
         {
            w.WriteLine("GROUP BY");
            w.WriteLine("   " + string.Join(",\r\n   ", meta.GroupByColumns.ToArray()));
         }
         if (meta.HavingClausePredicates.Any())
         {
            w.WriteLine("HAVING");
            w.WriteLine("   " + string.Join("\r\n   AND ", meta.HavingClausePredicates.Select(s => "(" + s + ")").ToArray()));
         }
         if (meta.OrderColumns.Any())
         {
            w.WriteLine("ORDER BY");
            w.WriteLine("   " + string.Join(",\r\n   ", meta.OrderColumns.ToArray()));
         }
         return this;
      }

      public virtual string DelimitIdentifier(string qualifiedName)
      {
         var replaceToken = IdentifierDelimiterLeft+"."+IdentifierDelimiterRight;
         return qualifiedName.StartsWith(IdentifierDelimiterLeft) ? qualifiedName : IdentifierDelimiterLeft + qualifiedName.Replace(".", replaceToken) + IdentifierDelimiterRight;

      }


   }
}