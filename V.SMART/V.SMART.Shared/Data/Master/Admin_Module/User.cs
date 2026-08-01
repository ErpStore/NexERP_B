using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Admin
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public int? StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff? Staff { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(100, ErrorMessage = "User Name cannot exceed 100 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(250, ErrorMessage = "Password cannot exceed 250 characters.")]
        public string UserPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 digits.")]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "User Role is required.")]
        public UserRole? Role { get; set; }

        public bool LevelAuthorization { get; set; } = false;

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? EmailId { get; set; }

        [StringLength(250)]
        public string? EmailAppPassword { get; set; }

        [StringLength(150)]
        public string? EmailServerName { get; set; }

        [StringLength(10)]
        public string? EmailPortNo { get; set; }

        public string? StateCodesCsv { get; set; }

        [NotMapped]
        public List<int> StateCodes
        {
            get => string.IsNullOrWhiteSpace(StateCodesCsv)
                ? new List<int>()
                : StateCodesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            set => StateCodesCsv = value != null && value.Any()
                ? string.Join(",", value)
                : null;
        }

        public bool IsViewOnly { get; set; } = false;

        public bool HideManualJobOrderButton { get; set; } = false;


        // Newley Added Fields
        public Guid? QrToken { get; set; }

        public bool IsQrEnabled { get; set; } = false;
        public bool IsProductionBased { get; set; } = false;

        public DateTime? QrCreatedDate { get; set; }

        public DateTime? QrExpiryDate { get; set; }

        public DateTime? LastQrLogin { get; set; }

        public int TrialDays { get; set; }
        public DateTime? ExpiryDate { get; set; }


        // Desktop 22072026----------------------------------
        public bool IsDesktop { get; set; } = false;

        [StringLength(100)]
        public string? DesktopName { get; set; }

        [StringLength(100)]
        public string? DesktopDeviceId { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        // Mobile
        public bool IsMobile { get; set; } = false;

        [StringLength(100)]
        public string? MobileName { get; set; }

        [StringLength(100)]
        public string? MobileDeviceId { get; set; }

        [StringLength(45)]
        public string? MobileIpAddress { get; set; }
        //---------------------------------------------------------


        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public ICollection<UserRight> UserRights { get; set; } = new List<UserRight>();
        public ICollection<UserAuthority> UserAuthorities { get; set; } = new List<UserAuthority>();
    }


}