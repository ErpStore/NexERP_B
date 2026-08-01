using DocumentFormat.OpenXml.Office.CoverPageProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel
{
    public class ApprovalVM
    {
        public int ID { get; set; }
        public string DocType { get; set; }
        public string DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }

        public int NoOfItems { get; set; }
        public decimal Amount { get; set; }

        public string Supplier { get; set; } 
        public string ItemName { get; set; } 
        public decimal Qty { get; set; }     

        public string Name { get; set; } // For Leave
        public string Reason { get; set; } // For Leave

        public DateTime? FromDate { get; set; } // Leave
        public DateTime? ToDate { get; set; }   // Leave

        public string? status { get; set; }

        public bool IsSelected { get; set; }
    }

}
