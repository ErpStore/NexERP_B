using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM
{
    public class MaterialReqPendingVM
    {
        public long SlNo { get; set; }
        public string? Customer { get; set; }
        public string? MReqNo { get; set; }
        public DateTime MreqDate { get; set; }
        public string? MainRemark { get; set; }
        
        public string? MreqStatus { get; set; }

        // Item Details
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal PurchQty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Remark { get; set; }
        public string? PONo {  get; set; }
        public DateTime? PODate { get; set; }
        public decimal Qty { get; set; }


    }
}
