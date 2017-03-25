using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XAdo.Core.SimpleJson;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;

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

      [Test]
      public void TestJsonAnnotation()
      {

         var obj = new JsonAnnotation
         {
            @readonly = true,
            crud = "CRUD",
            maxLength = 10,
            outputOnCreate = true,
            outputOnUpdate = true,
            type = "int",
            map = "../Address/Id",
            notnull = true,
            pkey = true
         };
         var json = SimpleJson.SerializeObject(obj);

         json =
            "{'onUpdate':3,'onCreate':1,'type':'int','maxLength':10,'map':'../Address/Id','readonly':true,'nullable':true,'pkey':true}";
         obj = SimpleJson.DeserializeObject<JsonAnnotation>(json);



      }
   }
}
