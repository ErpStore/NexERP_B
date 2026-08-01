using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel
{
    public class BOMAssemblySpVM
    {
       
        public int ItemId { get; set; }
       

        public decimal UtilQty { get; set; }

        public string? PartItemCode { get; set; }
        public string? PartItemName { get; set; }

        public string? PartSpecification { get; set; }
        public string? UOM { get; set; }
        public string? HSNCode { get; set; }

        public decimal BomStockQty { get; set; }

       
    }
}
