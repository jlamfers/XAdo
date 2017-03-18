using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XAdo.Core.SimpleJson;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class JSONTest
   {
      [Test]
      public void MonkeyTest()
      {
         var person = new {Name = "Jaap", Born = DateTime.Today.AddYears(-46), City = "Paris"};
         var json = SimpleJson.SerializeObject(person);
         var obj = SimpleJson.DeserializeObject("{name:'jaap',age:10}");

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            obj = SimpleJson.DeserializeObject("{name:'jaap',age:10}");
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }
   }
}
