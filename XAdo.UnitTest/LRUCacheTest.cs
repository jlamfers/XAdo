using NUnit.Framework;
using XAdo.Sql.Core;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class LRUCacheTest
   {
      [Test]
      public void MainTest()
      {
         var cache = new LRUCache<int, object>(500);

         for (var i = 0; i < 10000; i++)
         {
            cache.Add(i,i);
         }
         Assert.IsFalse(cache.ContainsKey(0));

         cache = new LRUCache<int, object>(500);

         for (var i = 0; i < 600; i++)
         {
            cache[i] = i;
         }
         Assert.IsFalse(cache.ContainsKey(0));

         cache = new LRUCache<int, object>(500);

         for (var i = 0; i < 600; i++)
         {
            cache[i] = i;
            var t = cache[0];
         }
         Assert.IsTrue(cache.ContainsKey(0));

         cache.Clear();
         Assert.IsFalse(cache.ContainsKey(0));
      }
   }
}
