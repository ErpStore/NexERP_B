using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    using V.SMART.Shared.Data.Master.General;
    using V.SMART.Shared.Data.Master.Inventory_module;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CustomerVM
    {
        public int CustId { get; set; }

        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(100, ErrorMessage = "Customer Name cannot exceed 100 characters.")]
        public string CustName { get; set; }

        [Required(ErrorMessage = "Customer Address is required.")]
        [StringLength(250, ErrorMessage = "Customer Address cannot exceed 250 characters.")]
        public string CustAddr { get; set; }

        [Required(ErrorMessage = "GST Number is required.")]
        [StringLength(15, ErrorMessage = "GST Number cannot exceed 15 characters.")]
        public string GSTNo { get; set; }

        [StringLength(10, ErrorMessage = "PAN Number must be 10 characters.")]
        public string? PANNo { get; set; }

        [Phone(ErrorMessage = "Please enter a valid contact number.")]
        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 characters.")]
        public string? ContactNo { get; set; }

        [Required(ErrorMessage = "State selection is required.")]
        public int StateId { get; set; }

        [Required(ErrorMessage = "State Name is required.")]
        [StringLength(100, ErrorMessage = "State Name cannot exceed 100 characters.")]
        public string StateName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Opening Balance must be a non-negative amount.")]
        [Precision(18, 2)]
        public decimal? OpenBal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Pending Balance must be a non-negative amount.")]
        [Precision(18, 2)]
        public decimal? OpenBalPndg { get; set; }

        public DateTime? OpenBalDate { get; set; }

        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string? Email { get; set; }



        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remark { get; set; }

        [Required(ErrorMessage = "PIN Code is required.")]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "PIN Code must be exactly 6 digits and not start with 0.")]
        [StringLength(6, ErrorMessage = "PIN Code must be 6 digits.")]
        public string PinCode { get; set; }

        [StringLength(100, ErrorMessage = "Scope cannot exceed 100 characters.")]
        public string? Scope { get; set; }

        [Required(ErrorMessage = "Business Type is required.")]
        [StringLength(50, ErrorMessage = "Business Type cannot exceed 50 characters.")]
        public string BusiType { get; set; }

        [StringLength(50, ErrorMessage = "TAN Number cannot exceed 50 characters.")]
        public string? TanNo { get; set; }

        [StringLength(50, ErrorMessage = "CIN Number cannot exceed 50 characters.")]
        public string? CINNo { get; set; }

        public bool RejectRetrac { get; set; }

        [Required(ErrorMessage = "Please select Currency Type")]
        public int? CurrId { get; set; }
        public string? CurrName { get; set; }

        [StringLength(500, ErrorMessage = "Bank Details cannot exceed 500 characters.")]
        public string? BankDetails { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Location must contain only letters and spaces.")]
        [StringLength(50, ErrorMessage = "Location cannot exceed 50 characters.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Distance is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Distance must be greater than 0.")]
        public double Distance { get; set; }

        [Precision(18, 3)]
        public decimal TDS { get; set; }
        [Precision(18, 3)]
        public decimal TDSExmptdAmt { get; set; }
        [Precision(18, 3)]
        public decimal TDSBillval { get; set; }
        [Precision(18, 3)]
        public decimal TDSDeductper { get; set; }

        public bool TdsDeduct { get; set; }
        public bool TdsDeductInSales { get; set; }
        public bool LineId { get; set; }
        public bool IsWoNoReq { get; set; }
        public bool TaxValidate { get; set; }

        [Required(ErrorMessage = "Customer Type is required.")]
        [StringLength(10, ErrorMessage = "Customer Type cannot exceed 10 characters.")]
        public string SupTyp { get; set; }


        public bool Inactive { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? OpeningBalancePending { get; set; }

        [StringLength(40, ErrorMessage = "Created By name cannot exceed 40 characters.")]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(40, ErrorMessage = "Modified By name cannot exceed 40 characters.")]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        public List<CustomerIndirectVM> CustomerIndirectVMs { get; set; } = new();
        public List<ContactPersonVM> ContactPersonVMs { get; set; } = new();
        public List<ItemSubVM> ItemSubVMs { get; set; } = new();


    }

}
