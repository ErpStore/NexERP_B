using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel
{
    public class UserAuthorityVM : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "UserName is Required")]
        public int? UserId { get; set; }
        public string? UserName { get; set; } = string.Empty;

        // ===== Manufacturing Quote =====
        public bool IsMfgQuote { get; set; }
        public string? IsMfgQuoteLevel { get; set; }

        // ===== Purchase Requisition =====
        public bool IsPR { get; set; }
        public string? IsPRLevel { get; set; }

        // ===== Purchase Order =====
        public bool IsPO { get; set; }
        public string? IsPOLevel { get; set; }

        // ===== Purchase SCN =====
        public bool IsPurchSCN { get; set; }
        public string? IsPurchSCNLevel { get; set; }

        // ===== SubCon SCN =====
        public bool IsSubConSCN { get; set; }
        public string? IsSubConSCNLevel { get; set; }

        // ===== Production Assembly SCN =====
        public bool IsProdAssySCN { get; set; }
        public string? IsProdAssySCNLevel { get; set; }

        // ===== Production Component SCN =====
        public bool IsProdCompSCN { get; set; }
        public string? IsProdCompSCNLevel { get; set; }

        // ===== Labour SCN =====
        public bool IsLabourSCN { get; set; }
        public string? IsLabourSCNLevel { get; set; }

        // ===== Leave =====
        public bool IsLeave { get; set; }
        public string? IsLeaveLevel { get; set; }

        // ===== Route Card =====
        public bool IsRc { get; set; }
        public string? IsRcLevel { get; set; }

        // ===== Mfg PO  =====
        public bool IsSalesPo { get; set; } = false;
        public string? IsSalesPoLevel { get; set; }

        // ===== Material Issue Request  =====
        public bool IsStockReq { get; set; } = false;
        public string? IsStockReqLevel { get; set; }

        // ================= CONDITIONAL VALIDATION =================
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (IsMfgQuote && string.IsNullOrWhiteSpace(IsMfgQuoteLevel))
                yield return new ValidationResult("Select MFG Quote Level", new[] { nameof(IsMfgQuoteLevel) });

            if (IsPR && string.IsNullOrWhiteSpace(IsPRLevel))
                yield return new ValidationResult("Select PR Level", new[] { nameof(IsPRLevel) });

            if (IsPO && string.IsNullOrWhiteSpace(IsPOLevel))
                yield return new ValidationResult("Select PO Level", new[] { nameof(IsPOLevel) });

            if (IsPurchSCN && string.IsNullOrWhiteSpace(IsPurchSCNLevel))
                yield return new ValidationResult("Select Purchase SCN Level", new[] { nameof(IsPurchSCNLevel) });

            if (IsSubConSCN && string.IsNullOrWhiteSpace(IsSubConSCNLevel))
                yield return new ValidationResult("Select SubCon SCN Level", new[] { nameof(IsSubConSCNLevel) });

            if (IsProdAssySCN && string.IsNullOrWhiteSpace(IsProdAssySCNLevel))
                yield return new ValidationResult("Select Production Assembly SCN Level", new[] { nameof(IsProdAssySCNLevel) });

            if (IsProdCompSCN && string.IsNullOrWhiteSpace(IsProdCompSCNLevel))
                yield return new ValidationResult("Select Production Component SCN Level", new[] { nameof(IsProdCompSCNLevel) });

            if (IsLabourSCN && string.IsNullOrWhiteSpace(IsLabourSCNLevel))
                yield return new ValidationResult("Select Labour SCN Level", new[] { nameof(IsLabourSCNLevel) });

            if (IsLeave && string.IsNullOrWhiteSpace(IsLeaveLevel))
                yield return new ValidationResult("Select Leave Level", new[] { nameof(IsLeaveLevel) });

            if (IsRc && string.IsNullOrWhiteSpace(IsRcLevel))
                yield return new ValidationResult("Select Route Card Level", new[] { nameof(IsRcLevel) });

        }
    }
}
