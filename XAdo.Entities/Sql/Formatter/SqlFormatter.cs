using System;
using System.IO;
using System.Linq;
using XAdo.Quobs.Descriptor;

namespace XAdo.Quobs.Sql.Formatter
{
   public class SqlFormatter : ISqlFormatter
   {

      public string ParameterPrefix { get; protected set; }
      public string StatementSeperator { get; protected set; }
      public string IdentifierDelimiterLeft { get; protected set; }
      public string IdentifierDelimiterRight { get; protected set; }

      public SqlFormatter()
      {
         ParameterPrefix = "@";
         StatementSeperator = ";";
         IdentifierDelimiterLeft = "\"";
         IdentifierDelimiterRight = "\"";

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

      public virtual ISqlFormatter FormatPageQuery(TextWriter writer, QueryDescriptor descriptor)
      {
         if (!descriptor.IsPaged())
         {
            descriptor.WriteSelect(writer);
            return this;
         }

         var sw = new StringWriter();
         if (!descriptor.OrderColumns.Any())
         {
            throw new InvalidOperationException("For SQL paging at least one order column must be specified.");
         }
         descriptor.WriteSelect(sw,true);
         var sql = sw.GetStringBuilder().ToString();
         var orderClause = String.Join(", ", descriptor.OrderColumns.Select(c => c.ToString()).ToArray());
         var parNameSkip = this.FormatParameterName(QueryDescriptor.Constants.ParNameSkip);
         var parNameTake = this.FormatParameterName(QueryDescriptor.Constants.ParNameTake);

         if (descriptor.Skip == null)
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
            writer.WriteLine(format, orderClause, sql, parNameSkip, parNameTake, String.Join(", ", descriptor.SelectColumns.Select(t => t.Alias ?? t.Expression).ToArray()));
         }
         return this;
      }


   }
}