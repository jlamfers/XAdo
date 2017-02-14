using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XAdo.Sql.Core;

namespace XAdo.Sql.Providers
{
   public class SqlServerDialect : SqlDialect
   {
      private IDictionary<Type, string> _typemap;

      public override string ProviderName { get { return "System.Data.SqlClient"; } }

      public  string SelectTemplate2
      {
         get { return @"
$(SELECT)
--$WHERE {where}
--$HAVING {having}
--$ORDER BY {order}
--$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY
"; }
      }
      public override string SelectTemplate
      {
         get
         {
            return @"
--${?skip}{?take}SELECT * FROM (
   $(SELECT-COLUMNS) 
     --${?skip}{?take},ROW_NUMBER() OVER (ORDER BY {order}) AS __rownum
   $(FROM...) 
   --$WHERE {where}
   --$HAVING {having}
   --${!skip}{!take}ORDER BY {order}
--$) AS __outer WHERE __rowNum > {skip} AND __rowNum <= {skip}+{take} ORDER BY __rowNum
";
         }
      }

      public override string IdentifierSeperator { get { return "."; } }
      public override string StatementSeperator { get { return ";"; } }
      public override string IdentifierDelimiterLeft { get { return "["; } }
      public override string IdentifierDelimiterRight { get { return "]"; } }
      public override string StringDelimiter { get { return "'"; } }
      public override string EscapedStringDelimiter { get { return "''"; } }

      public override string ParameterFormat { get { return "@{0}"; } }
      public override string DateTimeFormat { get { return "{0:yyyy-MM-dd HH:mm:ss.fff}"; } }
      public override string CharFormat { get { return "CHAR({0})"; } }
      public override string ExistsFormat { get { return "SELECT CAST( CASE WHEN EXISTS({0}\r\n) THEN 1 ELSE 0 END AS BIT)"; } }
      public override string CountFormat { get { return "SELECT COUNT(1) FROM ({0}\r\n) AS __tt_count"; } }

      public override string DateTimeNow { get { return "GETDATE()"; } }
      public override string DateTimeToday { get { return "CONVERT(DATE, GETDATE())"; } }
      public override string DateTimeUtcNow { get { return "GETUTCDATE()"; } }
      public override string TypeCast { get { return "CAST({0} AS {1})"; } }

      public override string Modulo { get { return "{0} % {1}"; } }

      public override string Power{get { return "POWER({0},{1})"; }}

      public override string StringLength { get { return "LEN({0})"; } }
      public override string StringToUpper { get { return "UPPER({0})"; } }
      public override string StringToLower { get { return "LOWER({0})"; } }
      public override string StringContains { get { return "({0} LIKE '%'+{1}+'%')"; } }
      public override string StringStartsWith { get { return "({0} LIKE {1}+'%')"; } }
      public override string StringEndsWith { get { return "({0} LIKE '%'+{1})"; } }

      public override string MathFloor { get { return "FLOOR({0})"; } }
      public override string MathRound { get { return "ROUND({0},...)"; } }
      public override string MathRoundZeroDecimals { get { return "ROUND({0},0)"; } }
      public override string MathCeiling { get { return "CEILING({0})"; } }
      public override string Coalesce { get { return "COALESCE({0,...})"; } }
      public override string StringConcat { get { return "{0+...}"; } }

      //NOTE: date at {0}, count at {1}
      public override string DateTimeAddDays { get { return "DATEADD(DAY,{1},{0})"; } }
      public override string DateTimeAddMonths { get { return "DATEADD(MONTH,{1},{0})"; } }
      public override string DateTimeAddYears { get { return "DATEADD(YEAR,{1},{0})"; } }
      public override string DateTimeAddHours { get { return "DATEADD(HOUR,{1},{0})"; } }
      public override string DateTimeAddMinutes { get { return "DATEADD(MINUTE,{1},{0})"; } }
      public override string DateTimeAddSeconds { get { return "DATEADD(SECOND,{1},{0})"; } }
      public override string DateTimeAddMilliSeconds { get { return "DATEADD(MILLISECOND,{1},{0})"; } }

      public override string DateTimeGetDay { get { return "DATEPART(DAY,{0})"; } }
      public override string DateTimeGetMonth { get { return "DATEPART(MONTH,{0})"; } }
      public override string DateTimeGetYear { get { return "DATEPART(YEAR,{0})"; } }
      public override string DateTimeGetHour { get { return "DATEPART(HOUR,{0})"; } }
      public override string DateTimeGetMinute { get { return "DATEPART(MINUTE,{0})"; } }
      public override string DateTimeGetSecond { get { return "DATEPART(SECOND,{0})"; } }
      public override string DateTimeGetMilliSecond { get { return "DATEPART(MILLISECOND,{0})"; } }

      public override string DateTimeGetDate { get { return "CONVERT(DATE, {0})"; } }
      public override string DateTimeGetDayOfWeek { get { return "(DATEPART(WEEKDAY,{0}) - 1)"; } } // => compliant with DayOfWeek enumeration
      public override string DateTimeGetDayOfYear { get { return "DATEPART(DAYOFYEAR,{0})"; } }
      public override string DateTimeGetWeekNumber { get { return "DATEPART(ISOWK,{0})"; } }

      public override string SelectLastIdentity { get { return "SELECT SCOPE_IDENTITY()"; } }
      public override string SelectLastIdentityTyped { get { return "SELECT CAST(SCOPE_IDENTITY() AS {0})"; } }

      public override string BitwiseNot { get { return "~({0})"; } }
      public override string BitwiseAnd { get { return "{0} & {1}"; } }
      public override string BitwiseOr { get { return "{0} | {1}"; } }
      public override string BitwiseXOR { get { return "{0} ^ {1}"; } }


      public override IDictionary<Type, string> TypeMap
      {
         get
         {
            return _typemap ?? (_typemap = new ReadOnlyDictionary<Type, string>(new Dictionary<Type, string>
             {
                {typeof(Guid),"UNIQUEIDENTIFIER"},
                {typeof(bool),"BIT"},
                {typeof(byte),"TINYINT"},
                {typeof(sbyte),"SMALLINT"},
                {typeof(short),"SMALLINT"},
                {typeof(ushort),"INT"},
                {typeof(int),"INT"},
                {typeof(uint),"BIGINT"},
                {typeof(long),"BIGINT"},
                {typeof(ulong),"DECIMAL(20)"},
                {typeof(decimal),"DECIMAL(29,4)"},
                {typeof(float),"REAL"},
                {typeof(double),"FLOAT"},
                {typeof(char),"CHAR(1)"},
                {typeof(string),"NVARCHAR(MAX)"},
                {typeof(DateTime),"DATETIME"},
                {typeof(DateTimeOffset),"DATETIMEOFFSET"},
                {typeof(TimeSpan),"TIME"},
                {typeof(byte[]),"VARBINARY(MAX)"},
             }));
         }
      }

   }
}
