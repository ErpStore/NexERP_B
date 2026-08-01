using V.SMART.Shared.Data.Master.General;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Accounts_Module
{
    public class CostCenter
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select Type of project first?.")]
        public int? ProjectTypeId { get; set; }

        [ForeignKey("ProjectTypeId")]
        public ProjectTypeMaster? ProjectTypeMaster { get; set; }

        [Required(ErrorMessage = "Sorry project No. should not be empty.")]
        [StringLength(15, ErrorMessage = "Project No. must be less than 15 characters.")]
        public string ProjectNo { get; set; }

        [Required(ErrorMessage = "Sorry Cost Center/Project Name should not be empty.")]
        [StringLength(150, ErrorMessage = "Cost Center/Project Name must be less than 150 characters.")]
        public string CostCenterName { get; set; }
        [Required(ErrorMessage = "Please select a customer.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid customer.")]
        public int? CustId { get; set; }

        [ForeignKey("CustId")]
        public Customer? Customer { get; set; }


        [Precision(18, 2)]
        public decimal AllocatedBudget { get; set; }
        [Precision(18, 2)]
        public decimal TotalBudget { get; set; }
        [Precision(18, 2)]
        public decimal AvailableBudget { get; set; }

        [StringLength(80)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now; // Indian time default

        [StringLength(80)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsClosed { get; set; }
        public bool IsDispatch { get; set; }

        [StringLength(15)]
        public string? ProjectStatus { get; set; }

        [StringLength(200, ErrorMessage = "Description must be less than 200 characters.")]
        public string? Description { get; set; }

        public bool AssToEnq { get; set; }
        public bool AssToQuote { get; set; }
        public bool AssToMfgPO { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Budgets
        [Precision(18, 2)]
        public decimal BroughtOutMechanical { get; set; } = 0;
        [Precision(18, 2)]
        public decimal MechanicalFabricated { get; set; } = 0;
        [Precision(18, 2)]
        public decimal CEFieldElement { get; set; } = 0;
        [Precision(18, 2)]
        public decimal CePanelElement { get; set; } = 0;
        [Precision(18, 2)]

        // Department Allocations
        public decimal MechanicalDesign { get; set; } = 0;
        [Precision(18, 2)]
        public decimal ElectricalDesign { get; set; } = 0;
        [Precision(18, 2)]
        public decimal PlcofflineProgram { get; set; } = 0;
        [Precision(18, 2)]
        public decimal SoftwareDesign { get; set; } = 0;
        [Precision(18, 2)]
        public decimal MechanicalAssembly { get; set; } = 0;
        [Precision(18, 2)]
        public decimal ElectricalWiring { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Commision { get; set; } = 0;
        [Precision(18, 2)]
        public decimal PLCOnlineProgram { get; set; } = 0;

        [Required(ErrorMessage = "Please select manager.")]
        [StringLength(80)]
        public string ProjectManager { get; set; }


        // Expense Breakdown
        [Precision(18, 2)]
        public decimal RawMaterial { get; set; } = 0;
        [Precision(18, 2)]
        public decimal ToolingCost { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Fabrication { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Machining { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Finishing { get; set; } = 0;
        [Precision(18, 2)]
        public decimal DesignCost { get; set; } = 0;
        [Precision(18, 2)]
        public decimal AssemblyFAT { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Installation { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Documentation { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Logistics { get; set; } = 0;
        [Precision(18, 2)]
        public decimal FinancialCost { get; set; } = 0;
        [Precision(18, 2)]
        public decimal AdminOverHead { get; set; } = 0;
        [Precision(18, 2)]
        public decimal Others { get; set; } = 0;
        [Precision(18, 2)]
        public decimal ConsumableAmt { get; set; } = 0;
        [Precision(18, 2)]
        public decimal MaintenanceAmt { get; set; } = 0;
        [Precision(18, 2)]
        public decimal SafetyAmt { get; set; } = 0;
        [Precision(18, 2)]
        public decimal ServiceDabaspetAmt { get; set; } = 0;
        [Precision(18, 2)]
        public decimal ServiceHalolAmt { get; set; } = 0;
        [Precision(18, 2)]
        public decimal CapexAmt { get; set; } = 0;
    }
}
