namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    using System;
    using System.ComponentModel.DataAnnotations;

    namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
    {
        public class Staff
        {
            [Key]
            public int StaffID { get; set; }

            [Required, StringLength(150)]
            public string? StaffRefId { get; set; }

            [Required, StringLength(150)]
            public string? StaffName { get; set; }

            [Required]
            public DateTime? DOB { get; set; }

            [Required, StringLength(25)]
            public string? Gender { get; set; }

            [Required, StringLength(150)]
            public string? Designation { get; set; }

            [Required, StringLength(150)]
            public string? Department { get; set; }

            [Required]
            public DateTime? DOJ { get; set; }

            [Required, StringLength(13)]
            public string? PhoneNo { get; set; }

            [StringLength(50)]
            public string? Gmail { get; set; }

            [Required, StringLength(250)]
            public string? PresentAddr { get; set; }

            [StringLength(250)]
            public string? PermanentAddr { get; set; }

            [Required, StringLength(100)]
            public string? RelationType { get; set; }

            [StringLength(150)]
            public string? FatherHusbName { get; set; }

            [Required, StringLength(150)]
            public string? City { get; set; }

            public float Height { get; set; }

            [StringLength(50)]
            public string? BloodGroup { get; set; }

            [StringLength(25)]
            public string? Criteria { get; set; }

            [Required, StringLength(25)]
            public string? Reporting { get; set; }

            [StringLength(250)]
            public string? LngRead { get; set; }

            [StringLength(250)]
            public string? LngWrite { get; set; }

            [StringLength(250)]
            public string? LngSpeak { get; set; }

            public float TotExp { get; set; }
            public float RelExp { get; set; }

            [Required, StringLength(15)]
            public string? Status { get; set; }

            [StringLength(20)]
            public string? WorkingHrs { get; set; }

            [StringLength(20)]
            public string? GraceHrs { get; set; }

            public bool OtCount { get; set; }
            public bool IsUser { get; set; }

            public string? OtherDet { get; set; }
            public byte[]? Img { get; set; }

            [StringLength(12)]
            public string? UANNo { get; set; }

            [StringLength(12)]
            public string? Aadhaar { get; set; }

            [StringLength(10)]
            public string? PAN { get; set; }

            [StringLength(10)]
            public string? ESICodeNo { get; set; }

            [StringLength(20)]
            public string? PFCodeNo { get; set; }

            [StringLength(150)]
            public string? Password { get; set; }

            [StringLength(18)]
            public string? AccNo { get; set; }

            [StringLength(11)]
            public string? IFSC { get; set; }

            [StringLength(150)]
            public string? BankName { get; set; }

            [StringLength(250)]
            public string? BranchName { get; set; }

            [StringLength(250)]
            public string? BankAddress { get; set; }

            [StringLength(50)]
            public string? BankState { get; set; }

            [StringLength(25)]
            public string? DepartmentCode { get; set; }

            [StringLength(40)]
            public string? CreatedBy { get; set; }

            public DateTime? CreatedDate { get; set; }

            [StringLength(40)]
            public string? ModifiedBy { get; set; }

            public DateTime? ModifiedDate { get; set; }

            // Navigation
            public ICollection<StaffEducation>? StaffEducations { get; set; }
            public ICollection<StaffEmergency>? StaffEmergencies { get; set; }
            public ICollection<StaffFamilyDetails>? StaffFamilyDetails { get; set; }
        }

    }

}
