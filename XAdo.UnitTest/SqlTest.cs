using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using NUnit.Framework;
using XAdo.Sql;
using XAdo.Sql.Core;
using XAdo.Sql.Providers;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class SqlTest
   {

      public const string SqlDialect = @"
{
  ""IdentifierSeperator"": ""."",
  ""StatementSeperator"": "";"",
  ""IdentifierDelimiterLeft"": ""["",
  ""IdentifierDelimiterRight"": ""]"",
  ""StringDelimiter"": ""'"",
  ""EscapedStringDelimiter"": ""''"",
  ""ParameterFormat"": ""@{0}"",
  ""DateTimeFormat"": ""{0:yyyy-MM-dd HH:mm:ss.fff}"",
  ""Now"": ""GETDATE()"",
  ""Today"": ""CONVERT(DATE, GETDATE())"",
  ""UtcNow"": ""GETUTCDATE()"",
  ""Exists"": ""SELECT CAST( CASE WHEN EXISTS({0}) THEN 1 ELSE 0 END AS BIT)"",
  ""TypeCast"": ""CAST({0} AS {1})"",
  ""Modulo"": ""{0} % {1}"",
  ""Power"": ""POWER({0},{1})"",
  ""StringLength"": ""LEN({0})"",
  ""ToUpper"": ""UPPER({0})"",
  ""ToLower"": ""LOWER({0})"",
  ""Contains"": ""({0} LIKE '%'+{1}+'%')"",
  ""StartsWith"": ""({0} LIKE {1}+'%')"",
  ""EndsWith"": ""({0} LIKE '%'+{1})"",
  ""Floor"": ""FLOOR({0})"",
  ""Round"": ""ROUND({0},...)"",
  ""RoundZeroDecimals"": ""ROUND({0},0)"",
  ""Ceiling"": ""CEILING({0})"",
  ""Coalesce"": ""COALESCE({0,...})"",
  ""Concat"": ""{0+...}"",
  ""DateTimeAddDay"": ""DATEADD(DAY,{1},{0})"",
  ""DateTimeAddMonth"": ""DATEADD(MONTH,{1},{0})"",
  ""DateTimeAddYear"": ""DATEADD(YEAR,{1},{0})"",
  ""DateTimeAddHour"": ""DATEADD(HOUR,{1},{0})"",
  ""DateTimeAddMinute"": ""DATEADD(MINUTE,{1},{0})"",
  ""DateTimeAddSecond"": ""DATEADD(SECOND,{1},{0})"",
  ""DateTimeAddMilliSecond"": ""DATEADD(MILLISECOND,{1},{0})"",
  ""DateTimeGetDay"": ""DATEPART(DAY,{0})"",
  ""DateTimeGetMonth"": ""DATEPART(MONTH,{0})"",
  ""DateTimeGetYear"": ""DATEPART(YEAR,{0})"",
  ""DateTimeGetHour"": ""DATEPART(HOUR,{0})"",
  ""DateTimeGetMinute"": ""DATEPART(MINUTE,{0})"",
  ""DateTimeGetSecond"": ""DATEPART(SECOND,{0})"",
  ""DateTimeGetMilliSecond"": ""DATEPART(MILLISECOND,{0})"",
  ""DateTimeGetDate"": ""CONVERT(DATE, {0})"",
  ""DateTimeGetWeekDay"": ""(DATEPART(WEEKDAY,{0}) - 1)"",
  ""DateTimeGetDayOfYear"": ""DATEPART(DAYOFYEAR,{0})"",
  ""DateTimeGetWeekNumber"": ""DATEPART(ISOWK,{0})"",
  ""SelectLastIdentity"": ""SELECT SCOPE_IDENTITY()"",
  ""SelectLastIdentityTyped"": ""SELECT CAST(SCOPE_IDENTITY() AS {0})"",
  ""BitwiseNot"": ""~({0})"",
  ""BitwiseAnd"": ""{0} & {1}"",
  ""BitwiseOr"": ""{0} | {1}"",
  ""BitwiseXOR"": ""{0} ^ {1}"",
  ""TypeMap"": {
    ""System.Guid"": ""UNIQUEIDENTIFIER"",
    ""System.Boolean"": ""BIT"",
    ""System.Byte"": ""TINYINT"",
    ""System.SByte"": ""SMALLINT"",
    ""System.Int16"": ""SMALLINT"",
    ""System.UInt16"": ""INT"",
    ""System.Int32"": ""INT"",
    ""System.UInt32"": ""BIGINT"",
    ""System.Int64"": ""BIGINT"",
    ""System.UInt64"": ""DECIMAL(20)"",
    ""System.Decimal"": ""DECIMAL(29,4)"",
    ""System.Single"": ""REAL"",
    ""System.Double"": ""FLOAT"",
    ""System.Char"": ""CHAR(1)"",
    ""System.String"": ""NVARCHAR(MAX)"",
    ""System.DateTime"": ""DATETIME"",
    ""System.DateTimeOffset"": ""DATETIMEOFFSET"",
    ""System.TimeSpan"": ""TIME"",
    ""System.Byte[]"": ""VARBINARY(MAX)""
  }
}
";
      public class Foo
      {
         public int Id { get; set; }
         public string Name { get; set; }
         public DateTime Born { get; set; }
      }
      [Test]
      public void MonkeyTest()
      {
         var sql = @"
SELECT
 rowguid as ID,
 FirstName+' '+LastName as Name,
 Born
FROM
 Person
--$WHERE {where}
";
         var sp = new SelectParser();
         var tuples = sp.ParseSelectColumns(sql);
         var map = tuples.Columns.ToDictionary(t => t.Alias, t => t.Name);
         var b = new SqlBuilder(new SqlServerDialect(),true);
         string oops="";
         var result = b.Parse(Predicate<Foo>(e => e.Born < DateTime.Now && (e.Name ?? oops) != null && e.Name.CompareTo("a") > 0 || e.Name.Contains("oops")),map);
         var resultSql = sql.FormatSqlTemplate(new {where = result.Sql});
         Debug.WriteLine(resultSql);
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            b.Parse(Predicate<Foo>(e => e.Born < DateTime.Now && (e.Name ?? oops) != null && e.Name.AsComparable() > "a" || e.Name.Contains("oops")), map);
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }

      private static Expression<Func<T, bool>> Predicate<T>(Expression<Func<T, bool>> e)
      {
         return e;
      }

      [Test]
      public void SerializeTest()
      {
         var d = new SqlServerDialect();
         var result = JsonConvert.SerializeObject(d, Formatting.Indented);

         var des = JsonConvert.DeserializeObject<SqlServerDialect>(SqlDialect);


      }
   }
}
