using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel
{
    public class JobOrderVM : IValidatableObject
    {
        public int JobId { get; set; }

        [Required(ErrorMessage = "Select Manufacturing or Labour type.")]
        public bool MfgORLab { get; set; } = true;

        public bool IsManual { get; set; }

        [Required(ErrorMessage = "Job No is required.")]
        [StringLength(20, ErrorMessage = "Job No cannot exceed 20 characters.")]
        public string JobNo { get; set; }
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Job Date is required.")]
        public DateTime JobDate { get; set; } = DateTime.Now;


        public CustomerVM? SelectedCustomer { get; set; }
        public int? CustId { get; set; }

        [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters.")]
        public string? CustomerName { get; set; }

        [StringLength(50, ErrorMessage = "Ref PO No cannot exceed 50 characters.")]
        public string? RefPoNo { get; set; }

        public DateTime? RefPODate { get; set; }
        public int? RefPoId { get; set; }
        public int? RefPoSubId { get; set; }
        public int? LineNo { get; set; }


        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "PO Qty must be valid number.")]
        public decimal POQty { get; set; }

        [Precision(18,3)]
        [Range(0.001, double.MaxValue, ErrorMessage = "Job Order Qty must be greater than zero.")]
        public decimal JobOrderQty { get; set; }

        [Precision(18, 3)]
        public decimal JobBalQty { get; set; }


        public bool JobTally { get; set; }

        [StringLength(300, ErrorMessage = "Remark cannot exceed 300 characters.")]
        public string? Remark { get; set; }


        public ItemVM? SelectedAssemblyItem { get; set; }

        [Required(ErrorMessage = "Select Assembly Item.")]
        public int? AssyId { get; set; }
        public string? AssemblyItemCode { get; set; }

        [Precision(18, 3)]
        public decimal UtilQty { get; set; }


        public bool Cancel { get; set; }

        [StringLength(300, ErrorMessage = "Cancel Reason cannot exceed 300 characters.")]
        public string? CancelReason { get; set; }

        public int? ParentJobId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? StaffID { get; set; }
        public string? StaffName { get; set; }
        public string? DepartmentCode { get; set; }

        public bool IsLineNoRequired { get; set; } = false;

        [MinLength(1, ErrorMessage = "At least one JobOrder item is required.")]
        [ValidateComplexType]
        public List<JobOrderSubVM> JobOrderSubVMs { get; set; } = new();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsManual && CustId == null)
            {
                yield return new ValidationResult(
                    "Customer selection is required when job is not manual.",
                    new[] { nameof(CustId) }
                );
            }

            if (!IsManual && POQty > 0 && JobOrderQty > POQty)
            {
                yield return new ValidationResult(
                    $"Job Order Qty ({JobOrderQty}) cannot exceed PO Qty ({POQty}).",
                    new[] { nameof(JobOrderQty) }
                );
            }

            if (IsManual && string.IsNullOrWhiteSpace(Remark))    
            {
                yield return new ValidationResult(
                    "The Remark field is required when the job card is manual.",
                    new[] { nameof(Remark) }
                );
            }
        }


    }
}
