using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core
{
   public static class FormatterExtension
   {
      public static string FormatColumn(this ISqlFormatter formatter, DbSchemaDescriptor.ColumnDescriptor c)
      {
         return formatter.FormatColumn(c.Parent.Schema, c.Parent.Name, c.Name,null);
      }

      public static string FormatColumn(this ISqlFormatter formatter, DbSchemaDescriptor.ColumnDescriptor c, string alias)
      {
         return formatter.FormatColumn(c.Parent.Schema, c.Parent.Name, c.Name, alias);
      }

      public static string FormatTable(this ISqlFormatter formatter, DbSchemaDescriptor.TableDescriptor t)
      {
         return formatter.FormatTable(t.Schema, t.Name,null);
      }

      public static string FormatJoin(this ISqlFormatter formatter, string expression)
      {
         if (formatter.IdentifierDelimiterLeft == "[") return expression;
         return expression
            .Replace("[", formatter.IdentifierDelimiterLeft)
            .Replace("]", formatter.IdentifierDelimiterRight);
      }
   }
}
