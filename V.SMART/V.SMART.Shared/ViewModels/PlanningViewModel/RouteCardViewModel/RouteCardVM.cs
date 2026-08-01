using DocumentFormat.OpenXml.Spreadsheet;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel
{
    public class RouteCardVM
    {

        public int RCId { get; set; }
        public bool MfgOrLab { get; set; } = true;
        public bool IsManual { get; set; } = false;

        [Required(ErrorMessage = "Route Card No is required.")]
        [StringLength(50, ErrorMessage = "Route Card No cannot exceed 50 characters.")]
        public string RCNo { get; set; }

        [Required(ErrorMessage = "Route Card Suffix is required.")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Route Card Date is required.")]
        public DateTime RCDate { get; set; } = DateTime.Now;
        public DateTime? RCDateNow { get; set; } = DateTime.Now;
        // Qty
        [Precision(18, 3)]
        public decimal RcQty { get; set; }

        public byte RcType { get; set; }  // 1=Auto, 2=Manual
        public byte RcStatus { get; set; }  // 0=Pending 1=InProgress 2=Completed 3=Cancelled


        public int? CustId { get; set; }
        public string CustName { get; set; }
        public CustomerVM? SelectedCustomer { get; set; }


        public int? RefPoId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }


        public int? RefRcReleseId { get; set; }
        public string? RefRcReleaseNo { get; set; }
        public DateTime? RefRcReleaseDate { get; set; }


        public string? SelectedItemType { get; set; }

        [Required(ErrorMessage = "Select Component Item is required")]
        public int? CompItemId { get; set; }
        public string? CompItemCode { get; set; }
        public List<ItemVM> CompItemVMs { get; set; } = new();
        public ItemVM? SelectedComponent { get; set; }


        public int? AssyId { get; set; }
        public string? AssyCode { get; set; }
        public List<ItemVM> AssyItemVMs { get; set; } = new();

        public int? SubAssyId { get; set; }
        public string? SubAssyCode { get; set; }
        public List<ItemVM> SubAssyItemVMs { get; set; } = new();


        public int? SubAssyId2 { get; set; }
        public string? SubAssyCode2 { get; set; }
        public List<ItemVM> SubAssyItemVM2s { get; set; } = new();


        public int? SubAssyId3 { get; set; }
        public string? SubAssyCode3 { get; set; }
        public List<ItemVM> SubAssyItemVM3s { get; set; } = new();


        [Required(ErrorMessage = "Select Raw Material is required")]
        public int? RMId { get; set; }
        public string? RMCode { get; set; }
        public List<ItemVM> RMItemVMs { get; set; } = new();



        [Precision(18, 3)]
        public decimal RMReqQty { get; set; }
        [Precision(18, 3)]
        public decimal RMStockQty { get; set; }

        [Precision(18, 3)]
        public decimal RMWeight { get; set; }
        public string? RMBatchNo { get; set; }

        public string? MainRemarks { get; set; }

        // Media
        public byte[]? BarCodeImage { get; set; }
        public byte[]? QrCodeImage { get; set; }

        public string? BarCodeBase64 =>
            BarCodeImage != null ? $"data:image/png;base64,{Convert.ToBase64String(BarCodeImage)}" : null;

        public string? QrCodeBase64 =>
            QrCodeImage != null ? $"data:image/png;base64,{Convert.ToBase64String(QrCodeImage)}" : null;



        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        // Audit
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Child List
        [MinLength(1, ErrorMessage = "At least one RC Process is required.")]
        [ValidateComplexType]
        public List<RouteCardSubVM> RouteCardSubVMs { get; set; } = new();

    }
}
