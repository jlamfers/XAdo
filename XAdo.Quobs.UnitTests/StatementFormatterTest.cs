using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XAdo.Quobs.Dialect.Core;

namespace XAdo.Quobs.UnitTests
{
   [TestFixture]
   public class StatementFormatterTest
   {

      [Test]
      public void Test()
      {
         var s = new StatementFormatter("");
         Debug.Write(s.Format());

         s = new StatementFormatter("Dit is een test");
         Debug.WriteLine(s.Format());

         s = new StatementFormatter("Dit is een {0} test dus");
         Debug.WriteLine(s.Format(w => w.Write("MOEILIJKE")));

         s = new StatementFormatter("{0}Dit is een {0} test dus{0}");
         Debug.WriteLine(s.Format(w => w.Write("!")));

         s = new StatementFormatter("{0}Dit is een {0} test dus{ 0 }");
         Debug.WriteLine(s.Format(w => w.Write("!")));

         //s = new StatementFormatter("{0} dit zijn de argumenten: {1/...}");
         //Debug.WriteLine(s.Format("en",1,2,3));

         s.Format("?");

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            s.Format(w => w.Write("!"));
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            "{0}Dit is een {0} test dus{ 0 }".Format(w => w.Write("!"));
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);


         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            string.Format("{0}Dit is een {0} test dus{0}", "!");
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }
   }
}
