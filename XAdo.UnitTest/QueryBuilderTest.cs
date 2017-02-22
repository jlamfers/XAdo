using System.Diagnostics;
using NUnit.Framework;
using Sql.Parser;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class QueryBuilderTest
   {

      [Test]
      public void MonkeyTest()
      {
         var p = QueryBuilder.Parse("");
         var s = p.Format(null);
         p = QueryBuilder.Parse(@"
SELECT -- oops
a.b.c--oops
--oops
, --oops
a --oops
FROM --oops
t --oops
");
         s = p.Format(null);

      }

      [Test]
      public void MonkeyTest2()
      {
         var p = QueryBuilder.Parse(@"
WITH cte AS 
-- oops
--(
   SELECT * FROM tTABLE
)
SELECT jan,jaap
FROM cte
ORDER BY jan
");
         var s = p.Format(null);
         Debug.WriteLine(s);

      }
   }
}
