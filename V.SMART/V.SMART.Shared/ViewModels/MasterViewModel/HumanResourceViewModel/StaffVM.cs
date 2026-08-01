using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class StaffVM
    {
        public int StaffID { get; set; }

        [Required(ErrorMessage = "Employee Code is required")]
        [StringLength(150, ErrorMessage = "Employee Code cannot exceed 150 characters")]
        [Display(Name = "Employee Code")]
        public string? StaffRefId { get; set; }

        [Required(ErrorMessage = "Employee Name is required")]
        [StringLength(150, ErrorMessage = "Employee Name cannot exceed 150 characters")]
        [Display(Name = "Employee Name")]
        public string? StaffName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(25)]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [StringLength(150)]
        public string? Designation { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(150)]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Date of Joining is required")]
        [DataType(DataType.Date)]
        public DateTime? DOJ { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string? PhoneNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(50)]
        public string? Gmail { get; set; }

        [Required(ErrorMessage = "Present address is required")]
        [StringLength(250)]
        public string? PresentAddr { get; set; }

        [StringLength(250)]
        public string? PermanentAddr { get; set; }

        [Required(ErrorMessage = "Relation type is required")]
        public string? RelationType { get; set; }

        [StringLength(150)]
        public string? FatherHusbName { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string? City { get; set; }

        [Range(0, 300, ErrorMessage = "Invalid height")]
        public float Height { get; set; }

        public string? BloodGroup { get; set; }

        public string? Criteria { get; set; }

        [Required(ErrorMessage = "Reporting person is required")]
        public string? Reporting { get; set; }

        public string? LngRead { get; set; }
        public string? LngWrite { get; set; }
        public string? LngSpeak { get; set; }

        [Range(0, 60, ErrorMessage = "Invalid experience")]
        public float TotExp { get; set; }

        [Range(0, 60, ErrorMessage = "Invalid relevant experience")]
        public float RelExp { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string? Status { get; set; }

        public string? WorkingHrs { get; set; }
        public string? GraceHrs { get; set; }

        public bool OtCount { get; set; }
        public bool IsUser { get; set; }

        public string? OtherDet { get; set; }
        public byte[]? Img { get; set; }

        // ================= Government IDs =================

        [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar must be 12 digits")]
        public string? Aadhaar { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format")]
        public string? PAN { get; set; }

        [StringLength(12)]
        public string? UANNo { get; set; }

        public string? ESICodeNo { get; set; }
        public string? PFCodeNo { get; set; }

        // ================= Bank Details =================

        [StringLength(18)]
        public string? AccNo { get; set; }

        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC Code")]
        public string? IFSC { get; set; }

        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? BankAddress { get; set; }
        public string? BankState { get; set; }
        public string? DepartmentCode { get; set; }


        // ================= Audit =================

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ================= Child Tables =================

        public List<StaffEducationVM> Educations { get; set; } = new();
        public List<StaffEmergencyVM> Emergencies { get; set; } = new();
        public List<StaffFamilyVM> Families { get; set; } = new();
    }
}
