using System.Diagnostics;
using NUnit.Framework;
using XAdo.Sql.Core;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class SelectParserTest
   {
      public class Args
      {
         public string Where { get; set; }
         public string Order { get; set; }
         public string Having { get; set; }
         public object Skip { get; set; }
         public object Take { get; set; }

         public bool? Paging
         {
            get { return (Skip != null && Take != null) ? (bool?)true : null; }
         }
         public bool? NoPaging
         {
            get { return Paging.GetValueOrDefault(false) ? null :(bool?) true;}
         }
      }

      [Test]
      public void MonkeyTest()
      {
         var sql = @"
--${?paging}SELECT * FROM (
SELECT 
     [FirstName],
     LastName as [c1] 
     --${?paging},ROW_NUMBER() OVER (ORDER BY {order}) AS __rownum
   FROM Person.Person 
   --$WHERE {where}
   --$HAVING {having}
   --${?nopaging}ORDER BY {order}
--${?paging}) WHERE __rowNum > {skip} AND __rowNum <= {skip}+{take} ORDER BY __rowNum
";

//   --$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY
   

         var p = new SelectParser();
         var tokens = p.ParseSelectColumns(sql);

         var result = sql.FormatSqlTemplate(new Args { Order = "name asc" });
         tokens = p.ParseSelectColumns(result);

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            result = sql.FormatSqlTemplate(new Args { Order = "name asc, street desc", Where = "name is not null", Skip = 10, Take = null });
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         Debug.WriteLine(result);

         Debug.WriteLine("".Substring(0,0));

      }
   }
}
