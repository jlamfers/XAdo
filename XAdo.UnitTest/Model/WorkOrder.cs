using System;
using XAdo.Quobs.Attributes;

namespace XAdo.UnitTest.Model
{
   [Quobs("Production.WorkOrder")]
    public partial class WorkOrder
    {
        public int WorkOrderID { get; set; }
        public int ProductID { get; set; }
        public int OrderQty { get; set; }
        public int StockedQty { get; set; }
        public short ScrappedQty { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public short? ScrapReasonID { get; set; }
        public DateTime ModifiedDate { get; set; }

        //public string TempName { get; set; }
        //public string X{ get; set; }
    }
}