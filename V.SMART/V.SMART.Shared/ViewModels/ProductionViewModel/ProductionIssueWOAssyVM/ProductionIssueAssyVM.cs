using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM
{
    public class ProductionIssueAssyVM
    {
        public int IssueId { get; set; }

        [Required(ErrorMessage = "Issue Number is required.")]
        [MaxLength(50, ErrorMessage = "Issue Number cannot exceed 50 characters.")]
        public string IssueNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? DepartmentCode { get; set; }

        [MaxLength(2)]
        public string? MonthCode { get; set; }//= DateTime.Now.Month.ToString("D2");

        [Required(ErrorMessage = "Issue Date is required.")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Current Issue Date is required.")]
        public DateTime IssueDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Issue to Whom is required.")]
        [MaxLength(100, ErrorMessage = "Issue To Whom cannot exceed 100 characters.")]
        public string IssueToWhom { get; set; }

        [Required(ErrorMessage = "Issue From Store is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Store selection is Required")]
        public int? StoreIssId { get; set; }

        public string? StoreIssName { get; set; }

        [Range (1, int.MaxValue, ErrorMessage = "Number of Items must be at least 1.")]
        public int NoOfItems => ProductionIssueAssySubVMs?.Count ?? 0;

        [MaxLength(300, ErrorMessage = "Remark cannot exceed 300 characters.")]
        public string? MainRemark { get; set; }

        public bool IssueTally { get; set; } = false;

        /// Audit Fields
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        [MinLength(1, ErrorMessage = "At least one Production Issue item is required.")]
        [ValidateComplexType]
        public List<ProductionIssueAssySubVM>? ProductionIssueAssySubVMs { get; set; } = new();

        public List<int> SelectedJobOrderIds { get; set; } = new();

    }

}
