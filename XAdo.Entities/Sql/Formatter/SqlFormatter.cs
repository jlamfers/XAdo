using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XAdo.Quobs.Attributes;

namespace XAdo.Quobs.Sql.Formatter
{
   public class SqlFormatter : ISqlFormatter
   {
      public const string 
         ParNameSkip = "___skip",
         ParNameTake = "___take";


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
         return parameterName.StartsWith(ParameterPrefix) ? parameterName  : ParameterPrefix + parameterName;
      }
      public virtual string FormatColumn(MemberInfo member)
      {
         if (member == null) throw new ArgumentNullException("member");
         var a = member.GetCustomAttribute<ColumnAttribute>();
         if (a != null)
         {
            if (a.ColumnNameParts != null)
            {
               return DelimitIdentifier(a.ColumnNameParts);
            }
            if (a.SqlExpression != null)
            {
               return a.SqlExpression;
            }
         }
         return DelimitIdentifier(member.Name);
      }
      public virtual string FormatTableName(Type entityType)
      {
         if (entityType == null) throw new ArgumentNullException("entityType");
         var a = entityType.GetCustomAttribute<TableAttribute>();
         if (a != null)
         {
            if (a.TableNameParts != null)
            {
               return DelimitIdentifier(a.TableNameParts);
            }
            if (a.SqlExpression != null)
            {
               return a.SqlExpression;
            }
         }
         return DelimitIdentifier(entityType.Name);
      }

      public virtual string ConcatenateSqlStatements(IEnumerable<string> statements)
      {
         return String.Join(StatementSeperator + "\r\n", statements.ToArray());
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

         var distinct =  meta.Distict ? "DISTINCT " : "";

         if (!meta.SelectColumns.Any())
         {
            w.WriteLine("SELECT {0}*", distinct);
         }
         else
         {
            w.WriteLine("SELECT {0}", distinct);
            w.WriteLine("   " + String.Join(",\r\n   ", meta.SelectColumns.Select(t => String.IsNullOrEmpty(t.Item2) ? t.Item1 : t.Item1 + " AS " + t.Item2)));
         }
         w.WriteLine("FROM {0}", meta.TableName);
         if (meta.WhereClausePredicates.Any())
         {
            w.WriteLine("WHERE");
            w.WriteLine("   " + String.Join("\r\n   AND ", meta.WhereClausePredicates.Select(s => "(" + s + ")").ToArray()));
         }
         if (meta.GroupByColumns.Any())
         {
            w.WriteLine("GROUP BY");
            w.WriteLine("   " + String.Join(",\r\n   ", meta.GroupByColumns.ToArray()));
         }
         if (meta.HavingClausePredicates.Any())
         {
            w.WriteLine("HAVING");
            w.WriteLine("   " + String.Join("\r\n   AND ", meta.HavingClausePredicates.Select(s => "(" + s + ")").ToArray()));
         }
         if (meta.OrderColumns.Any())
         {
            w.WriteLine("ORDER BY");
            w.WriteLine("   " + String.Join(",\r\n   ", meta.OrderColumns.ToArray()));
         }
         return this;
      }

      public virtual ISqlFormatter FormatSqlSelectPaged(TextWriter writer, ISqlSelectMeta meta, string parNameSkip, string parNameTake)
      {
         var sw = new StringWriter();
         var orderColumns = meta.OrderColumns.ToList();
         if (!orderColumns.Any())
         {
            throw new InvalidOperationException("For SQL paging at least one order column must be specified.");
         }
         meta.OrderColumns.Clear();
         FormatSqlSelect(sw,meta);
         meta.OrderColumns.AddRange(orderColumns);

         var sql = sw.GetStringBuilder().ToString();
         var orderClause = String.Join(", ", orderColumns.ToArray());
         parNameSkip = parNameSkip != null ? FormatParameterName(parNameSkip) : null;
         parNameTake = parNameTake != null ? FormatParameterName(parNameTake) : null;

         if (parNameSkip == null)
         {
            writer.Write("SELECT TOP({0}) * FROM ({1}) AS __tt ORDER BY {2}", parNameTake, sql, orderClause);
         }
         else
         {

            const string format = @"
WITH __t1 AS (
(
SELECT *,ROW_NUMBER() OVER (ORDER BY {0}) AS __rowNum
FROM ({1}) AS __t2
)
)
SELECT {4}
FROM __t1
WHERE __rowNum BETWEEN ({2} + 1) AND ({2} + {3})
";
            writer.WriteLine(format, orderClause, sql, parNameSkip, parNameTake, String.Join(", ", meta.SelectColumns.Select(t => t.Item2 ?? t.Item1).ToArray()));
         }
         return this;
      }

      public virtual ISqlFormatter FormatSqlSelectCount(TextWriter writer, ISqlSelectMeta meta)
      {
         string sql;
         using (var sw = new StringWriter())
         {
            var orderColumns = meta.OrderColumns.ToList();
            meta.OrderColumns.Clear();
            FormatSqlSelect(sw, meta);
            meta.OrderColumns.AddRange(orderColumns);
            sql = sw.GetStringBuilder().ToString();
         }

         writer.Write("SELECT COUNT(1) FROM ({0}) AS t", sql);
         return this;
      }

      public virtual string DelimitIdentifier(string qualifiedName)
      {
         if (qualifiedName.StartsWith(IdentifierDelimiterLeft))
         {
            return qualifiedName;
         }
         return IdentifierDelimiterLeft + qualifiedName + IdentifierDelimiterRight;
      }
      public virtual string DelimitIdentifier(IList<string> nameParts)
      {
         var sb = new StringBuilder();
         var sep = "";
         foreach (var name in nameParts)
         {
            sb.Append(sep);
            var delimited = name.StartsWith(IdentifierDelimiterLeft);
            if (!delimited)
               sb.Append(IdentifierDelimiterLeft);
            sb.Append(name);
            if (!delimited)
               sb.Append(IdentifierDelimiterRight);
            sep = ".";
         }
         return sb.ToString();
      }
   }
}