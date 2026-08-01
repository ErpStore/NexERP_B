using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel
{
    public class ProductionPendingVM
    {
      
            public int? SlNo { get; set; }

            public string? JobNo { get; set; }
            public string? JobDate { get; set; }

            public string? CustName { get; set; }

            public string? PONo { get; set; }
            public string? PODate { get; set; }

            public string? JobType { get; set; }

            public decimal? JobOrderQty { get; set; }
            public decimal? JobBalQty { get; set; }

            public string? ItemCode { get; set; }
            public string? ItemName { get; set; }

            public decimal? UtilQty { get; set; }
            public decimal? ReqQty { get; set; }
            public decimal? BalQty { get; set; }

            public string? AssyItemCode { get; set; }
            public string? AssyItemName { get; set; }

            public string? IssueNo { get; set; }
            public string? IssueDate { get; set; }

            public int? CustId { get; set; }
            public int? ItemId { get; set; }

            public decimal? Qty { get; set; }

            public int? RefPoSubId { get; set; }

            public int? RcSubId { get; set; }

            public int? AssyId { get; set; }

            public string? BatchNo { get; set; }

            public string? TransType { get; set; }

            //---------------------------------
            // Return Component
            //---------------------------------

            public string? ReturnNo { get; set; }

            public string? ReturnDate { get; set; }

            public string? RefPoNo { get; set; }

            public string? RefRcNo { get; set; }

            public string? RefIssNo { get; set; }

            //---------------------------------
            // Stores
            //---------------------------------

            public string? FROMSTORE { get; set; }

            public string? TOSTORE { get; set; }

            public string? AddStore { get; set; }

            //---------------------------------
            // SCN
            //---------------------------------

            public string? SCNNo { get; set; }

            public string? RefGrnNo { get; set; }

            public decimal? RejQty { get; set; }

            //---------------------------------
            // Return Assembly
            //---------------------------------

            public string? RefIssueNo { get; set; }

            public string? ReturnBy { get; set; }
        }



    
}
