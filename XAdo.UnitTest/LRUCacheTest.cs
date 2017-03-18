using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XAdo.Core;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class LRUCacheTest
   {
      static LRUCache<int,object> _cache1 = new LRUCache<int, object>(500);
      static LRUCache<int, object> _cache2 = new LRUCache<int, object>(500);

      public void Test1()
      {
         for (var i = 0; i < 10000; i++)
         {
            _cache1[i] = i;
         }
      }
      public void Test2()
      {
         for (var i = 0; i < 10000; i++)
         {
            _cache2[i] = i;
         }
      }

         
         [Test]
      public void MainTest()
      {
         var sw = new Stopwatch();
         sw.Start();
         var cache = new LRUCache<int, object>(500);

         for (var i = 0; i < 10000; i++)
         {
            cache.Add(i,i);
         }
         //Assert.IsFalse(cache.ContainsKey(0));

         cache = new LRUCache<int, object>(500);

         for (var i = 0; i < 600; i++)
         {
            cache[i] = i;
         }
         //Assert.IsFalse(cache.ContainsKey(0));

         cache = new LRUCache<int, object>(500);

         for (var i = 0; i < 600; i++)
         {
            cache[i] = i;
            var t = cache[0];
         }
         //Assert.IsTrue(cache.ContainsKey(0));

         cache.Clear();
         //Assert.IsFalse(cache.ContainsKey(0));
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }

      /*
      [Test]
      public void MainTest2()
      {
         var sw = new Stopwatch();
         sw.Start();
         using (var cache = new LRUCache2<int, object>(500))
         {

            for (var i = 0; i < 10000; i++)
            {
               cache.Add(i, i);
            }
            Thread.Sleep(100);
            Assert.IsFalse(cache.ContainsKey(0));
         }
         using (var cache = new LRUCache2<int, object>(500))
         {

            cache[-1] = -1;
            Assert.IsTrue(cache.ContainsKey(-1));
            for (var i = 0; i < 600; i++)
            {
               cache[i] = i;
            }
            Assert.IsFalse(cache.ContainsKey(0));
         }

         using(var cache = new LRUCache2<int, object>(500))
         {

            for (var i = 0; i < 700; i++)
            {
               cache[i] = i;
               var t = cache[0];
            }
            Assert.IsTrue(cache.ContainsKey(0));

            Thread.Sleep(100);
            cache.Clear();
            Assert.IsFalse(cache.ContainsKey(0));
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }
      */

      [Test]
      public void MultiThreadTest1()
      {

         var sw = new Stopwatch();
         sw.Start();
         Task.WaitAll(new Int16[20].Select(i => Task.Run(() => Test1())).ToArray());
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
      }

      [Test]
      public void MultiThreadTest2()
      {

         var sw = new Stopwatch();
         sw.Start();
         Task.WaitAll(new Int16[20].Select(i => Task.Run(() => Test2())).ToArray());
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
         Thread.Sleep(100);

      }

      [Test]
      public void TestTwice()
      {
         Test1();
         Test1();
      }

      [Test]
      public void MultiThreadTest3()
      {
         const int size = 1500;
         const int trips = 1000;
         const int cacheSize = 1000;
         const int sleep = 1;
         var guids = new Guid[size].Select(g => Guid.NewGuid()).ToArray();
         var cache = new LRUCache<Guid, object>(cacheSize);
         var r = new Random(Guid.NewGuid().GetHashCode());

         #region Actions
         Action action1 = () =>
         {
            for (var i = 0; i < trips; i++)
            {
               var idx = r.Next(0, size - 1);
               var guid = guids[idx];
               cache.GetOrAdd(guid, k => guid);
               Thread.Sleep(sleep);
            }
         };

         Action action2 = () =>
         {
            for (var i = 0; i < trips; i++)
            {
               var idx = r.Next(0, size - 1);
               var guid = guids[idx];
               object value;
               cache.TryRemove(guid, out value);
               Thread.Sleep(sleep);

            }
         };

         Action action3 = () =>
         {
            for (var i = 0; i < trips; i++)
            {
               var idx = r.Next(0, size - 1);
               var guid = guids[idx];
               cache.TryAdd(guid,guid);
               Thread.Sleep(sleep);

            }
         };

         Action action4 = () =>
         {
            for (var i = 0; i < trips; i++)
            {
               var idx = r.Next(0, size - 1);
               var guid = guids[idx];
               object value;
               cache.TryGetValue(guid, out value);
               Thread.Sleep(sleep);

            }
         };
         #endregion

         var sw = new Stopwatch();
         sw.Start();
         Task.WaitAll(new Task[]
         {
            Task.Run(action1),
            Task.Run(action2),
            Task.Run(action3),
            Task.Run(action4),
            Task.Run(action1),
            Task.Run(action2),
            Task.Run(action3),
            Task.Run(action4),
            Task.Run(action1),
            Task.Run(action2),
            Task.Run(action3),
            Task.Run(action4),
            Task.Run(action1),
            Task.Run(action2),
            Task.Run(action3),
            Task.Run(action4),
            Task.Run(action1),
            Task.Run(action2),
            Task.Run(action3),
            Task.Run(action4),
            Task.Run(action1),
            Task.Run(action2),
            Task.Run(action3),
            Task.Run(action4)
         });
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);
         Thread.Sleep(200);
         var cache2 = cache;
      }

   }
}
