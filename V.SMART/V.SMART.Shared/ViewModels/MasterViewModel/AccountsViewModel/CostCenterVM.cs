using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel
{
    public class CostCenterVM
    {
        public int Id { get; set; }

        // ==============================
        // PROJECT BASIC DETAILS
        // ==============================

        [Required(ErrorMessage = "Please select Type of project first.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Project Type.")]
        public int? ProjectTypeId { get; set; }

        public string? ProjectTypeName { get; set; }   // For Grid Display

        [Required(ErrorMessage = "Project No. should not be empty.")]
        [StringLength(15, ErrorMessage = "Project No. must be less than 15 characters.")]
        public string? ProjectNo { get; set; }

        [Required(ErrorMessage = "Cost Center/Project Name should not be empty.")]
        [StringLength(150, ErrorMessage = "Name must be less than 150 characters.")]
        public string? CostCenterName { get; set; }

        [Required(ErrorMessage = "Please select a customer.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid customer.")]
        public int? CustId { get; set; }

        public string? CustomerName { get; set; }   // For Grid Display

        // ==============================
        // BUDGET SUMMARY
        // ==============================

        [Range(0, double.MaxValue)]
        public decimal AllocatedBudget { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalBudget { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AvailableBudget { get; set; }

        // ==============================
        // STATUS
        // ==============================

        public bool IsClosed { get; set; }
        public bool IsDispatch { get; set; }

        [StringLength(15)]
        public string? ProjectStatus { get; set; }

        public bool AssToEnq { get; set; }
        public bool AssToQuote { get; set; }
        public bool AssToMfgPO { get; set; }

        // ==============================
        // DATES
        // ==============================

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // ==============================
        // PROJECT MANAGER
        // ==============================

        [Required(ErrorMessage = "Please select manager.")]
        [StringLength(80)]
        public string? ProjectManager { get; set; }

        // ==============================
        // DESCRIPTION
        // ==============================

        [StringLength(200)]
        public string? Description { get; set; }

        // ==============================
        // DEPARTMENT ALLOCATION
        // ==============================

        public decimal MechanicalDesign { get; set; }
        public decimal ElectricalDesign { get; set; }
        public decimal PlcofflineProgram { get; set; }
        public decimal SoftwareDesign { get; set; }
        public decimal MechanicalAssembly { get; set; }
        public decimal ElectricalWiring { get; set; }
        public decimal Commision { get; set; }
        public decimal PLCOnlineProgram { get; set; }

        // ==============================
        // EXPENSE BREAKDOWN
        // ==============================

        public decimal RawMaterial { get; set; }
        public decimal ToolingCost { get; set; }
        public decimal Fabrication { get; set; }
        public decimal Machining { get; set; }
        public decimal Finishing { get; set; }
        public decimal DesignCost { get; set; }
        public decimal AssemblyFAT { get; set; }
        public decimal Installation { get; set; }
        public decimal Documentation { get; set; }
        public decimal Logistics { get; set; }
        public decimal FinancialCost { get; set; }
        public decimal AdminOverHead { get; set; }
        public decimal Others { get; set; }
        public decimal ConsumableAmt { get; set; }
        public decimal MaintenanceAmt { get; set; }
        public decimal SafetyAmt { get; set; }
        public decimal ServiceDabaspetAmt { get; set; }
        public decimal ServiceHalolAmt { get; set; }
        public decimal CapexAmt { get; set; }

        // ==============================
        // AUDIT
        // ==============================

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
