using System;

namespace XAdo.UnitTest.Model
{
    public partial class WorkOrderWithFields
    {
        public int WorkOrderID;
        public int ProductID;
        public int OrderQty;
        public int StockedQty;
        public short ScrappedQty;
        public DateTime StartDate;
        public DateTime? EndDate;
        public DateTime DueDate;
        public short? ScrapReasonID;
        public DateTime ModifiedDate;
        //public string X;
    }
}