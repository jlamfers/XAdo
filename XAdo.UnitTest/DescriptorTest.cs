using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Quobs.Attributes;

namespace XAdo.UnitTest
{
   [TestClass]
   public class DescriptorTest
   {
      [TestMethod]
      public void MonkeyTest()
      {
         var baseTableType = typeof(Quobs.Generator.DbBaseTable);
         var list = new List<object>();
         foreach (
            var t in baseTableType.Assembly.GetTypes().Where(t => !t.IsAbstract && baseTableType.IsAssignableFrom(t)))
         {
            list.Add(t.GetDescriptor());
         }
      }
   }
}
