using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory_module;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.General
{
    public class Vendor
    {
        [Key]
        public int VendorCode { get; set; }

        [Required, StringLength(150)]
        public string VendorName { get; set; }

        [Required, StringLength(300)]
        public string VendorAddress { get; set; }

        [StringLength(10)]
        public string? PANNo { get; set; }

        [StringLength(15)]
        public string? ContactNo { get; set; }

        [StringLength(250)]
        public string? Scope { get; set; }

        [Required, StringLength(15)]
        public string GSTNo { get; set; }

        [StringLength(15)]
        public string? CustomerCode { get; set; }

        [Precision(18, 2)]
        public decimal? OpenBal { get; set; }

        [Precision(18, 2)]
        public decimal? OpenBalPndg { get; set; }

        public DateTime? OpenBalDate { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [Required, StringLength(50)]
        public string BusiType { get; set; }

        public bool Inactive { get; set; }
        public bool SSI { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(20)]
        public string? BankAccountNo { get; set; }

        [StringLength(11)]
        public string? BankIFSCCode { get; set; }

        [StringLength(100)]
        public string? BankBranchName { get; set; }

        [Required, StringLength(100)]
        public string StateName { get; set; }

        public int StateCode { get; set; }

        [ForeignKey(nameof(StateCode))]
        public State State { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [Required, StringLength(6)]
        public string PinCode { get; set; }

        [Precision(18, 3)]
        public decimal TDS { get; set; }

        [Precision(18, 3)]
        public decimal TDSExmptdAmt { get; set; }

        [Precision(18, 3)]
        public decimal TDSBillval { get; set; }

        [Precision(18, 3)]
        public decimal TDSDeductper { get; set; }

        public bool TdsDeduct { get; set; }
        public bool IsitPurch { get; set; } = true;
        public bool IsitSubCon { get; set; } = true;

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(50)]
        public string? TANNo { get; set; }

        [StringLength(50)]
        public string? CINNo { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public bool RejectionRetract { get; set; }
        public bool IsAllowAccRejRew { get; set; }

        public double Distance { get; set; }
        public bool TaxValidate { get; set; }

        [StringLength(500)]
        public string? TermsAndCondition { get; set; }

        [StringLength(50)]
        public string? YearOfEstd { get; set; }

        [StringLength(50)]
        public string? PreviousYearTurnover { get; set; }

        public int NoofEmployee { get; set; }

        [StringLength(50)]
        public string? ProdMfg { get; set; }

        public string? MajorCust { get; set; }
        public string? MfgFacility { get; set; }
        public string? InspectionFacility { get; set; }
        public string? AnyQualSysFoll { get; set; }
        public string? AnyProdApprvl { get; set; }

        [StringLength(5)]
        public string? CouldMeetQuantityReq { get; set; }

        [StringLength(50)]
        public string? CompnyType { get; set; }

        [StringLength(5)]
        public string? ManintngDocs { get; set; }

        [StringLength(5)]
        public string? FAIMaintained { get; set; }

        [StringLength(5)]
        public string? FODMaintained { get; set; }

        public int? CurrId { get; set; }

        [ForeignKey(nameof(CurrId))]
        public Currency Currency { get; set; }

        public ICollection<VendorContact> VendorContacts { get; set; } = new List<VendorContact>();
        public ICollection<VendorInDirect> VendorInDirects { get; set; } = new List<VendorInDirect>();
        public ICollection<ItemSub> ItemSubs { get; set; } = new List<ItemSub>();

    }
}
