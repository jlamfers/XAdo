using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XAdo.Quobs.Dialects.Core;

namespace XAdo.Quobs.Dialects.SqlServer
{
   public class SqlServerDialect : ISqlDialect
   {
      private IDictionary<Type, string> _typemap;

      public virtual string IdentifierSeperator { get { return "."; } }
      public virtual string StatementSeperator { get { return ";"; } }
      public virtual string IdentifierDelimiterLeft { get { return "["; } }
      public virtual string IdentifierDelimiterRight { get { return "]"; } }
      public virtual string StringDelimiter { get { return "'"; } }
      public virtual string EscapedStringDelimiter { get { return "''"; } }
      public virtual string ParameterPrefix { get { return "@"; } }
      public virtual string DateTimeFormat { get { return "{0:yyyy-MM-dd HH:mm:ss.fff}"; } }
      public virtual string Now { get { return "GETDATE()"; } }
      public virtual string Today { get { return "CONVERT(DATE, GETDATE())"; } }
      public virtual string UtcNow { get { return "GETUTCDATE()"; } }
      public virtual string Exists { get { return "SELECT CAST( CASE WHEN EXISTS({0}) THEN 1 ELSE 0 END AS BIT)"; } }
      public virtual string TypeCast { get { return "CAST({0} AS {1})"; } }
      public virtual string Modulo { get { return "( {0} % {1} )"; } }
      public virtual string StringLength { get { return "LEN({0})"; } }
      public virtual string ToUpper { get { return "UPPER({0})"; } }
      public virtual string ToLower { get { return "LOWER({0})"; } }
      public virtual string Floor { get { return "FLOOR({0})"; } }
      public virtual string Round { get { return "ROUND({0},{1})"; } }
      public virtual string Ceiling { get { return "CEILING({0})"; } }
      public virtual string Coalesce { get { return "COALESCE({0,...})"; } }
      public virtual string Concat { get { return "({0+...})"; } }

      //NOTE: date at {0}, count at {1}
      public virtual string DateTimeAddDay { get { return "DATEADD(DAY,{1},{0})"; } }
      public virtual string DateTimeAddMonth { get { return "DATEADD(MONTH,{1},{0})"; } }
      public virtual string DateTimeAddYear { get { return "DATEADD(YEAR,{1},{0})"; } }
      public virtual string DateTimeAddHour { get { return "DATEADD(HOUR,{1},{0})"; } }
      public virtual string DateTimeAddMinute { get { return "DATEADD(MINUTE,{1},{0})"; } }
      public virtual string DateTimeAddSecond { get { return "DATEADD(SECOND,{1},{0})"; } }
      public virtual string DateTimeAddMilliSecond { get { return "DATEADD(MILLISECOND,{1},{0})"; } }

      public virtual string DateTimeGetDay { get { return "DATEPART(DAY,{0})"; } }
      public virtual string DateTimeGetMonth { get { return "DATEPART(MONTH,{0})"; } }
      public virtual string DateTimeGetYear { get { return "DATEPART(YEAR,{0})"; } }
      public virtual string DateTimeGetHour { get { return "DATEPART(HOUR,{0})"; } }
      public virtual string DateTimeGetMinute { get { return "DATEPART(MINUTE,{0})"; } }
      public virtual string DateTimeGetSecond { get { return "DATEPART(SECOND,{0})"; } }
      public virtual string DateTimeGetMilliSecond { get { return "DATEPART(MILLISECOND,{0})"; } }

      public virtual string DateTimeGetDate { get { return "CONVERT(DATE, {0})"; } }
      public virtual string DateTimeGetWeekDay { get { return "(DATEPART(WEEKDAY,{0}) - 1)"; } } // => compliant with DayOfWeek enumeration
      public virtual string DateTimeGetDayOfYear { get { return "DATEPART(DAYOFYEAR,{0})"; } }
      public virtual string DateTimeGetWeekNumber { get { return "DATEPART(ISOWK,{0})"; } }

      public virtual string SelectLastIdentity { get { return "SELECT SCOPE_IDENTITY()"; } }
      public virtual string SelectLastIdentityTyped { get { return "SELECT CAST(SCOPE_IDENTITY() AS {0})"; } }

      public virtual IDictionary<Type, string> TypeMap
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
