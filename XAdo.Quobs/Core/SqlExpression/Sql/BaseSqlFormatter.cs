using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core.SqlExpression.Sql
{
   public abstract class BaseSqlFormatter : ISqlFormatter
   {
      protected BaseSqlFormatter(IMemberFormatter memberFormatter)
      {
         if (memberFormatter == null) throw new ArgumentNullException("memberFormatter");
         MemberFormatter = memberFormatter;
         IdentifierSeperator = ".";
         StatementSeperator = ";";
         IdentifierDelimiterLeft = "\"";
         IdentifierDelimiterRight = "\"";
         ParameterPrefix = "@";

      }

      public IMemberFormatter MemberFormatter { get; private set; }
      public string IdentifierSeperator { get; protected set; }
      public string StatementSeperator { get; protected set; }
      public string IdentifierDelimiterLeft { get; protected set; }
      public string IdentifierDelimiterRight { get; protected set; }
      public string ParameterPrefix { get; protected set; }

      public virtual string FormatColumn(string schema, string table, string column, string alias)
      {
         using (var sw = new StringWriter())
         {
            this.FormatIdentifier(sw, schema, table, column);
            if (alias != null)
            {
               sw.Write(" AS ");
               this.FormatIdentifier(alias);
            }
            return sw.GetStringBuilder().ToString();
         }
      }
      public virtual string FormatTable(string schema, string table, string alias)
      {
         using (var sw = new StringWriter())
         {
            this.FormatIdentifier(sw, schema, table);
            if (alias != null)
            {
               sw.Write(" AS ");
               this.FormatIdentifier(alias);
            }
            return sw.GetStringBuilder().ToString();
         }
      }
      public virtual string FormatParameter(string parameterName)
      {
         return parameterName == null || parameterName.StartsWith(ParameterPrefix)
            ? parameterName
            : ParameterPrefix + parameterName;
      }

      public virtual Expression VisitorHook(ExpressionVisitor visitor, SqlBuilderContext context, Expression source)
      {
         return null;
      }

      public virtual object NormalizeValue(object value)
      {
         return value;
      }

      public abstract void WriteExists(TextWriter writer, Action<TextWriter> sqlSelect);

      public virtual void WriteTrueExpression(TextWriter writer)
      {
         writer.Write("(1 = 1)");
      }
      public virtual void WriteFalseExpression(TextWriter writer)
      {
         writer.Write("(1<>1)");
      }

      public abstract void WriteTypeCast(TextWriter writer, Type type, Action<TextWriter> value);

      public abstract string DateTimeFormat { get; }
      public abstract string Now { get; }
      public abstract string Today { get; }
      public abstract string UtcNow { get; }
      public abstract string FormatValue(object value);
      public abstract string FormatType(Type type);

      public abstract void WriteConcatenate(TextWriter writer, params Action<TextWriter>[] args);
      public abstract void WriteCoalesce(TextWriter writer, params Action<TextWriter>[] args);
      public abstract void WriteDateTimeWeekNumber(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeDate(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeYear(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeMonth(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeDay(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeDayOfWeek(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeDayOfYear(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeHour(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeMinute(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeSecond(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeMillisecond(TextWriter writer, Action<TextWriter> date);
      public abstract void WriteDateTimeAddDays(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteDateTimeAddYears(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteDateTimeAddMonths(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteDateTimeAddHours(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteDateTimeAddMinutes(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteDateTimeAddSeconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteDateTimeAddMilliseconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count);
      public abstract void WriteModulo(TextWriter writer, Action<TextWriter> left, Action<TextWriter> right);
      public abstract void WriteStringLength(TextWriter writer, Action<TextWriter> arg);
      public abstract void WriteToUpper(TextWriter writer, Action<TextWriter> arg);
      public abstract void WriteToLower(TextWriter writer, Action<TextWriter> arg);
      public abstract void WriteFloor(TextWriter writer, Action<TextWriter> arg);
      public abstract void WriteRound(TextWriter writer, Action<TextWriter> arg, Action<TextWriter> length);
      public abstract void WriteCeiling(TextWriter writer, Action<TextWriter> arg);

      /// <summary>
      /// Write a paged select query from the passed arguments
      /// </summary>
      /// <param name="writer">The writer to which the output is written</param>
      /// <param name="sqlSelectWithoutOrder">The fully formatted sql select query, without the order by clause</param>
      /// <param name="orderByClause">The fully formatted order columns</param>
      /// <param name="selectNames">The fully formatted single column names (no-dot-seperators, these may be aliases) that represent the returned column names</param>
      /// <param name="skip">The skip parameter, this may be either a formatted parameter name, or a formatted value</param>
      /// <param name="take">The take parameter, this may be either a formatted parameter name, or a formatted value</param>
      public virtual void WritePagedQuery(TextWriter writer, string sqlSelectWithoutOrder, IEnumerable<string> orderByClause, IEnumerable<string> selectNames, string skip, string take)
      {
         if (skip == null && take == null)
         {
            writer.WriteLine(sqlSelectWithoutOrder.TrimEnd());
            writer.Write("   ORDER BY ");
            writer.Write(orderByClause);
            return;
         }

         if (orderByClause == null || !orderByClause.Any())
         {
            throw new InvalidOperationException("For SQL paging at least one order column must be specified.");
         }

         if (skip == null)
         {
            writer.Write("SELECT TOP({0}) * FROM ({1}) AS __tt ORDER BY {2}", take, sqlSelectWithoutOrder, string.Join(", ",orderByClause.ToArray()));
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
            writer.WriteLine(format, string.Join(", ", orderByClause.ToArray()), sqlSelectWithoutOrder, skip, take, String.Join(", ", selectNames.ToArray()));
         }
      }


   }

   public static class SqlFormatterExtension
   {
      public static string FormatIdentifier(this ISqlFormatter self, params string[] identifiers)
      {
         using (var sw = new StringWriter())
         {
            self.FormatIdentifier(sw, identifiers);
            return sw.GetStringBuilder().ToString();
         }
      }
      public static ISqlFormatter FormatIdentifier(this ISqlFormatter self, TextWriter w, params string[] identifiers)
      {
         string sep = null;
         foreach (var i in identifiers)
         {
            if (i == null) continue;
            w.Write(sep);
            var delimited = i.StartsWith(self.IdentifierDelimiterLeft);
            if (!delimited)
               w.Write(self.IdentifierDelimiterLeft);
            w.Write(i);
            if (!delimited)
               w.Write(self.IdentifierDelimiterRight);
            sep = sep ?? self.IdentifierSeperator;
         }
         return self;
      }
   }
}
