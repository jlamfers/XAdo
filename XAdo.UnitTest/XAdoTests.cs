using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.UnitTest.Model;

namespace XAdo.UnitTest
{
    [TestClass]
    public class XAdoTests
    {
        //TODO....

        [TestMethod]
        public void WorkOrdersCanBeQueriedDynamically()
        {
            using (var session = Db.Northwind.CreateSession())
            {
                var list = session.Query("SELECT * FROM [Production].[WorkOrder]");
                var sw = new Stopwatch();
                sw.Start();
                list = session.Query("SELECT * FROM [Production].[WorkOrder]");
                sw.Stop();
                Debug.WriteLine("#rows fetched: "+list.Count()+", elapsed: " + sw.ElapsedMilliseconds+" ms.");
            }
        }

        [TestMethod]
        public void WorkOrdersCanBeQueriedWithFields()
        {
            using (var session = Db.Northwind.CreateSession())
            {
                var list = session.Query<WorkOrderWithFields>("SELECT * FROM [Production].[WorkOrder]");
                var sw = new Stopwatch();
                sw.Start();
                list = session.Query<WorkOrderWithFields>("SELECT * FROM [Production].[WorkOrder]");
                sw.Stop();
                Debug.WriteLine("#rows fetched: " + list.Count() + ", elapsed: " + sw.ElapsedMilliseconds + " ms.");
            }
        }

        [TestMethod]
        public void WorkOrdersCanBeQueriedWithProperties()
        {
            using (var session = Db.Northwind.CreateSession())
            {
                var list = session.Query<WorkOrder>("SELECT * FROM [Production].[WorkOrder]");
                var sw = new Stopwatch();
                sw.Start();
                list = session.Query<WorkOrder>("SELECT * FROM [Production].[WorkOrder]");
                sw.Stop();
                Debug.WriteLine("#rows fetched: " + list.Count() + ", elapsed: " + sw.ElapsedMilliseconds + " ms.");
            }
        }
    }
}
