﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects.Core;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.Dialects
{
   public class SqlFormatter : ISqlFormatter
   {

      public SqlFormatter(ISqlDialect dialect)
      {
         SqlDialect = dialect;
      }

      public ISqlDialect SqlDialect { get; private set; }

      public virtual void FormatIdentifier(TextWriter w, params string[] identifiers)
      {
         string sep = null;
         foreach (var i in identifiers)
         {
            if (i == null) continue;
            w.Write(sep);
            var delimited = i.StartsWith(SqlDialect.IdentifierDelimiterLeft);
            if (!delimited)
               w.Write(SqlDialect.IdentifierDelimiterLeft);
            w.Write(i);
            if (!delimited)
               w.Write(SqlDialect.IdentifierDelimiterRight);
            sep = sep ?? SqlDialect.IdentifierSeperator;
         }
      }
      public virtual void FormatColumn(TextWriter w, string schema, string table, string column, string alias)
      {
         FormatIdentifier(w, schema, table, column);
         if (alias != null)
         {
            w.Write(" AS ");
            FormatIdentifier(w, alias);
         }
      }
      public virtual void FormatTable(TextWriter w, string schema, string table, string alias)
      {
         FormatIdentifier(w, schema, table);
         if (alias != null)
         {
            w.Write(" AS ");
            FormatIdentifier(w, alias);
         }
      }
      public virtual void FormatParameter(TextWriter w, string parameterName)
      {
         if (string.IsNullOrEmpty(parameterName)) return;
         if (!parameterName.StartsWith(SqlDialect.ParameterPrefix))
         {
            w.Write(SqlDialect.ParameterPrefix);
         }
         w.Write(parameterName);
      }

      public virtual Expression VisitorHook(ExpressionVisitor visitor, SqlBuilderContext context, Expression source)
      {
         return null;
      }

      public virtual object NormalizeValue(object value)
      {
         return value;
      }

      public virtual void WriteExists(TextWriter writer, Action<TextWriter> sqlSelect)
      {
         SqlDialect.Exists.Format(writer, sqlSelect);
      }
      public virtual void WriteTrue(TextWriter writer)
      {
         writer.Write("(1 = 1)");
      }
      public virtual void WriteFalse(TextWriter writer)
      {
         writer.Write("(1<>1)");
      }
      public virtual void WriteTypeCast(TextWriter writer, Type type, Action<TextWriter> value)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         string sqlType;
         if (!SqlDialect.TypeMap.TryGetValue(type, out sqlType))
         {
            value(writer);
            return;
         }
         SqlDialect.TypeCast.Format(writer, value, w => FormatType(w,type));
      }
      public virtual void FormatType(TextWriter writer, Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         string sqlType;
         if (!SqlDialect.TypeMap.TryGetValue(type, out sqlType))
         {
            throw new SqlObjectsException(string.Format("Type {0} not supported.", type));
         }
         writer.Write(sqlType);
      }
      public virtual void FormatValue(TextWriter writer, object value)
      {
         if (value == null)
         {
            writer.Write("NULL");
            return;
         }

         value = NormalizeValue(value);

         switch (Type.GetTypeCode(value.GetType()))
         {
            case TypeCode.Char:
               var ch = (char)value;
               if (Char.IsControl(ch))
                  writer.Write("CHAR({0})", (int) ch);
               else
                  writer.Write("'{0}'", ch);
               return;
            case TypeCode.String:
               writer.Write("{0}{1}{0}", SqlDialect.StringDelimiter, EscapeString((string)value));
               return;
            case TypeCode.DBNull:
            case TypeCode.Empty:
               writer.Write("NULL");
               return;
            case TypeCode.Object:
               FormatValue(writer,value.ToString());
               return;
            case TypeCode.Boolean:
               writer.Write((bool)value ? "1" : "0");
               return;
            case TypeCode.SByte:
               writer.Write( ToString<SByte>(value));
               return;
            case TypeCode.Byte:
               writer.Write(ToString<Byte>(value));
               return;
            case TypeCode.Int16:
               writer.Write(ToString<Int16>(value));
               return;
            case TypeCode.UInt16:
               writer.Write(ToString<UInt16>(value));
               return;
            case TypeCode.Int32:
               writer.Write(ToString<Int32>(value));
               return;
            case TypeCode.UInt32:
               writer.Write(ToString<UInt32>(value));
               return;
            case TypeCode.Int64:
               writer.Write(ToString<Int64>(value));
               return;
            case TypeCode.UInt64:
               writer.Write(ToString<UInt64>(value));
               return;
            case TypeCode.Single:
               writer.Write(ToString<Single>(value));
               return;
            case TypeCode.Double:
               writer.Write(ToString<Double>(value));
               return;
            case TypeCode.Decimal:
               writer.Write(ToString<Decimal>(value));
               return;
            case TypeCode.DateTime:
               FormatValue(writer, string.Format(SqlDialect.DateTimeFormat, (DateTime)value));
               return;
         }
         throw new ArgumentOutOfRangeException();
         
      }
      protected virtual string ToString<T>(object value)
      {
         return string.Format(CultureInfo.InvariantCulture, "{0}", (T)value);
      }
      protected virtual string EscapeString(string value)
      {
         if (string.IsNullOrWhiteSpace(value)) return value;
         return value.Replace(SqlDialect.StringDelimiter, SqlDialect.EscapedStringDelimiter);
      }
      public virtual void WriteConcatenate(TextWriter writer, params Action<TextWriter>[] args)
      {
         SqlDialect.Concat.Format(writer,args);
      }
      public virtual void WriteCoalesce(TextWriter writer, params Action<TextWriter>[] args)
      {
         SqlDialect.Coalesce.Format(writer,args);
      }
      public virtual void WriteDateTimeWeekNumber(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetWeekNumber.Format(writer,date);
      }
      public virtual void WriteDateTimeDate(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetDate.Format(writer,date);
      }
      public virtual void WriteDateTimeYear(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetYear.Format(writer,date);
      }
      public virtual void WriteDateTimeMonth(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetMonth.Format(writer, date);
         
      }
      public virtual void WriteDateTimeDay(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetDay.Format(writer, date);
         
      }
      public virtual void WriteDateTimeDayOfWeek(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetWeekDay.Format(writer, date);
         
      }
      public virtual void WriteDateTimeDayOfYear(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetDayOfYear.Format(writer, date);
         
      }
      public virtual void WriteDateTimeHour(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetHour.Format(writer, date);

      }
      public virtual void WriteDateTimeMinute(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetMinute.Format(writer, date);

      }
      public virtual void WriteDateTimeSecond(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetSecond.Format(writer, date);

      }
      public virtual void WriteDateTimeMillisecond(TextWriter writer, Action<TextWriter> date)
      {
         SqlDialect.DateTimeGetMilliSecond.Format(writer, date);

      }
      public virtual void WriteDateTimeAddDays(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddDay.Format(writer, date, count);
      }
      public virtual void WriteDateTimeAddYears(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddYear.Format(writer, date, count);
         
      }
      public virtual void WriteDateTimeAddMonths(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddMonth.Format(writer, date, count);
      }
      public virtual void WriteDateTimeAddHours(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddHour.Format(writer, date, count);
      }
      public virtual void WriteDateTimeAddMinutes(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddMinute.Format(writer, date, count);
      }
      public virtual void WriteDateTimeAddSeconds(TextWriter writer, Action<TextWriter> date, Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddSecond.Format(writer, date, count);
      }
      public virtual void WriteDateTimeAddMilliseconds(TextWriter writer, Action<TextWriter> date,Action<TextWriter> count)
      {
         SqlDialect.DateTimeAddMilliSecond.Format(writer, date, count);
      }
      public virtual void WriteModulo(TextWriter writer, Action<TextWriter> left, Action<TextWriter> right)
      {
         SqlDialect.Modulo.Format(writer,left, right);
      }
      public virtual void WriteStringLength(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.StringLength.Format(writer,arg);
      }
      public virtual void WriteToUpper(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.ToUpper.Format(writer,arg);
      }
      public virtual void WriteToLower(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.ToLower.Format(writer,arg);
      }
      public virtual void WriteFloor(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.Floor.Format(writer,arg);
      }
      public virtual void WriteRound(TextWriter writer, Action<TextWriter> arg, Action<TextWriter> length)
      {
         SqlDialect.Round.Format(writer,arg,length);
      }
      public virtual void WriteCeiling(TextWriter writer, Action<TextWriter> arg)
      {
         SqlDialect.Ceiling.Format(writer,arg);
      }

      public virtual void WriteSelectLastIdentity(TextWriter writer)
      {
         writer.Write(SqlDialect.SelectLastIdentity);
      }
      public virtual void WriteSelectLastIdentity(TextWriter writer, Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         string sqlType;
         if (!SqlDialect.TypeMap.TryGetValue(type, out sqlType))
         {
            throw new SqlObjectsException("Cannot format type " + type + " with SqlFormatter " + GetType());
         }
         writer.Write(SqlDialect.SelectLastIdentityTyped, sqlType);
      }

      public virtual void WriteSelect(TextWriter writer, QueryChunks chuncks, bool ignoreOrder = false)
      {

         var distinct = chuncks.Distict ? "DISTINCT " : "";

         chuncks.EnsureOrderByColumnsAreAliased();

         if (!chuncks.SelectColumns.Any())
         {
            writer.WriteLine("SELECT {0}*", distinct);
         }
         else
         {
            writer.WriteLine("SELECT {0}", distinct);
            writer.WriteLine("   " +
                        String.Join(",\r\n   ",
                           chuncks.SelectColumns.Select(
                              t => String.IsNullOrEmpty(t.Alias) ? t.Expression : t.Expression + " AS " + t.Alias)));
         }
         writer.WriteLine("FROM {0} AS {1}", chuncks.TableName, this.FormatIdentifier(chuncks.Aliases.Table(0)));
         if (chuncks.Joins.Any())
         {
            writer.WriteLine("   " +
                        String.Join("\r\n   ",
                           chuncks.Joins.Select(j => j.ToString()).ToArray()));
         }
         if (chuncks.WhereClausePredicates.Any())
         {
            writer.WriteLine("WHERE");
            writer.WriteLine("   " +
                        String.Join("\r\n   AND ",
                           chuncks.WhereClausePredicates
                              .Select(s => "(" + s + ")")
                              .ToArray()));
         }
         if (chuncks.GroupByColumns.Any())
         {
            writer.WriteLine("GROUP BY");
            writer.WriteLine("   " + String.Join(",\r\n   ", chuncks.GroupByColumns.ToArray()));
         }
         if (chuncks.HavingClausePredicates.Any())
         {
            writer.WriteLine("HAVING");
            writer.WriteLine("   " +
                        String.Join("\r\n   AND ",
                           chuncks.HavingClausePredicates.Select(s => "(" + s + ")").ToArray()));
         }
         foreach (var union in chuncks.Unions)
         {
            writer.WriteLine("UNION");
            writer.WriteLine(union.GetSql());
         }
         if (!ignoreOrder && chuncks.OrderColumns.Any())
         {
            writer.WriteLine("ORDER BY");
            writer.WriteLine("   " + String.Join(",\r\n   ", chuncks.OrderColumns.Select(c => c.ToString()).ToArray()));
         }
      }
      public virtual void WriteCount(TextWriter writer, QueryChunks descriptor)
      {
         writer.Write("SELECT COUNT(1) FROM (");
         WriteSelect(writer, descriptor, true);
         writer.Write(") AS __pt_inner");
      }
      public virtual void WritePagedCount(TextWriter writer, QueryChunks chunks)
      {
         if (!chunks.IsPaged())
         {
            WriteCount(writer, chunks);
            return;
         }

         string sqlSelect;
         using (var w = new StringWriter())
         {
            var selectOrderColumns = !chunks.SelectColumns.Any();
            if (selectOrderColumns)
            {
               var index = 0;
               chunks.SelectColumns.AddRange(chunks.OrderColumns.Select(c => new QueryChunks.SelectColumn(c.Expression, chunks.Aliases.Column(index++))));
            }
            WriteSelect(w, chunks, true);
            if (selectOrderColumns)
            {
               chunks.SelectColumns.Clear();
            }
            sqlSelect = w.GetStringBuilder().ToString();
         }
         WritePagedQuery(
            writer,
            sqlSelect,
            chunks.OrderColumns.Select(c => c.Alias + (c.Descending ? " DESC" : "")),
            new[] { "COUNT(1)" },
            chunks.Skip != null ? this.FormatParameter(QueryChunks.Constants.ParNameSkip) : null,
            chunks.Take != null ? this.FormatParameter(QueryChunks.Constants.ParNameTake) : null);
      }
      public virtual void WritePagedSelect(TextWriter writer, QueryChunks chunks)
      {
         string sqlSelect;
         using (var w = new StringWriter())
         {
            WriteSelect(w, chunks, true);
            sqlSelect = w.GetStringBuilder().ToString();
         }
         WritePagedQuery(
            writer,
            sqlSelect,
            chunks.OrderColumns.Select(c => c.Alias + (c.Descending ? " DESC" : "")),
            chunks.SelectColumns.Select(c => c.Alias),
            chunks.Skip != null ? this.FormatParameter(QueryChunks.Constants.ParNameSkip) : null,
            chunks.Take != null ? this.FormatParameter(QueryChunks.Constants.ParNameTake) : null);
      }


      /// <summary>
      /// Write a paged select query from the passed arguments
      /// </summary>
      /// <param name="writer">The writer to which the output is written</param>
      /// <param name="sqlSelectWithoutOrder">The fully formatted sql select query, without the order by clause</param>
      /// <param name="orderByClause">The fully formatted order columns</param>
      /// <param name="selectNames">The fully formatted single column names (no-dot-seperators, these may be aliases) that represent the returned column names</param>
      /// <param name="skip">The skip parameter, this may be either a formatted parameter name, or a formatted value</param>
      /// <param name="take">The take parameter, this may be either a formatted parameter name, or a formatted value</param>
      protected virtual void WritePagedQuery(TextWriter writer, string sqlSelectWithoutOrder, IEnumerable<string> orderByClause, IEnumerable<string> selectNames, string skip, string take)
      {
         if (skip == null && take == null)
         {
            writer.WriteLine(sqlSelectWithoutOrder.TrimEnd());
            writer.Write("   ORDER BY ");
            writer.Write(string.Join(", ", orderByClause.ToArray()));
            return;
         }

         if (orderByClause == null || !orderByClause.Any())
         {
            throw new SqlObjectsException("For SQL paging at least one order column must be specified.");
         }

         if (skip == null)
         {
            writer.Write("SELECT TOP({0}) * FROM ({1}) AS __pt_outer ORDER BY {2}", take, sqlSelectWithoutOrder, string.Join(", ", orderByClause.ToArray()));
         }
         else
         {

            const string format = @"
WITH __pt_outer AS (
(
SELECT *,ROW_NUMBER() OVER (ORDER BY {0}) AS __rowNum
FROM ({1}) AS __pt_inner
)
)
SELECT {2}
FROM __pt_outer
";

            writer.WriteLine(format, string.Join(", ", orderByClause.ToArray()), sqlSelectWithoutOrder, String.Join(", ", selectNames.ToArray()));

            if (take == null)
            {
               writer.WriteLine("WHERE __rowNum > {0}", skip);
            }
            else
            {
               writer.WriteLine("WHERE __rowNum > {0} AND __rowNum <= ({0} + {1})", skip, take);
            }
         }
      }
   }

}
