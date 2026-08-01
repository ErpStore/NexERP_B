using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class LeaveApplication
    {
        [Key]
        public int LeaveAppID { get; set; }
        [Required(ErrorMessage = "EmployeeID is Required...")]

        public string LeaveApplicationNo { get; set; }
        public int EmployeeID { get; set; }

        [ForeignKey(nameof(EmployeeID))]
        public Staff Staff { get; set; }

        public string? Department { get; set; }

        [Required(ErrorMessage = "LeaveType name is Required...")]
        public int LeaveTypeId { get; set; }
        [ForeignKey(nameof(LeaveTypeId))]
        public LeaveType LeaveType { get; set; }

        [Required(ErrorMessage = "FromDate is Required...")]
        public DateTime FromDate { get; set; }= DateTime.Now;

        [Required(ErrorMessage = "ToDate is Required...")]
        public DateTime ToDate { get; set; }= DateTime.Now;


        [Precision(18, 2)]
        [Required(ErrorMessage = "TotalDays is Required...")]
        public decimal TotalDays { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal BalDaysBeforeLeaveApp { get; set; }

        public string? LeaveStatus { get; set; }
        [Required(ErrorMessage = "Reason is Required...")]
        public string Reason { get; set; }
        [Required(ErrorMessage = "AppliedDate is Required...")]
        public DateTime AppliedDate { get; set; } = DateTime.Now;

        //public bool IsView {  get; set; }
        public bool Level1 { get; set; }
        public bool Level2 { get; set; }
        public bool Level3 { get; set; }
        public string? Level1_Sign { get; set; }
        public string? Level2_sign { get; set; }
        public string? Level3_sign { get; set; }
        public bool Authorized { get; set; } = false;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public String? RejectReason { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = null;

        public List<DailyLeaveRecord> DailyLeaveRecords { get; set; } = new List<DailyLeaveRecord>();
        public List<Staff> Employee { get; set; }
    }
}
