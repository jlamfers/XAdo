using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sql.Parser;
using Sql.Parser.Tokens;

namespace XAdo.UnitTest
{


   [TestFixture]
   public class ParserTest
   {
      public const string Query3 = @"
SELECT  distinct     
/* Dit is 
   /* een test */*/
   p.BusinessEntityID as [id], -->*
   p.PersonType, -->! 
   p.Title, 
   p.FirstName, -->! 
   p.MiddleName, 
   p.LastName, -->! 
   p.ModifiedDate as ModifiedAt, 
   a.AddressID, --> Address/Id* 
   a.AddressLine1 as Line1, -->! 
   a.AddressLine2 as Line2, 
   a.PostalCode, -->! 
   a.ModifiedDate as ModifiedAt,
   at.AddressTypeID, -->AddressType/Id*
   at.Name, -->!
   a.City -->../City!
FROM Person.BusinessEntity AS be 
INNER JOIN Person.Person AS p ON be.BusinessEntityID = p.BusinessEntityID 
INNER JOIN Person.BusinessEntityAddress AS bea ON be.BusinessEntityID = bea.BusinessEntityID 
INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID 
INNER JOIN Person.AddressType AS at ON bea.AddressTypeID = at.AddressTypeID
--$WHERE {where}
--${!order}{!inner}ORDER BY p.BusinessEntityID
--$ORDER BY {order} 
--$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY

";

      [Test]
      public void MonkeyTest()
      {
         var tokens = Parse(@"
Select name from person 
where name not is null --$AND name <> {name}
group by name having name > 0 order by (name desc), name");
         var s = tokens.Format(new{name="[Name]"});

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            s = tokens.Format(new { name = "[Name]" });
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }

      [Test]
      public void MonkeyTest2()
      {
         var tokens = Parse(Query3);
         var s = tokens.Format(new { skip = 1, take=10 });

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            s = tokens.Format(new { skip = 1, take = 10 });
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }
      private IList<SqlToken> Parse(string sql)
      {
         var p = new SqlSelectParser();
         return p.Tokenize(sql);
      }
   }
}
