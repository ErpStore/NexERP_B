using V.SMART.Shared.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel
{
    public class UserVM : IValidatableObject
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "User Name must be between 3 and 100 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string UserPassword { get; set; } = string.Empty;

        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "User Role is required.")]
        public UserRole Role { get; set; }

        public int? StaffId { get; set; }
        public string? StaffRefId { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public bool LevelAuthorization { get; set; } = false;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(150)]
        public string? EmailId { get; set; }

        [StringLength(250)]
        public string? EmailAppPassword { get; set; }

        public string? EmailConfirmAppPassword { get; set; }

        [StringLength(150)]
        public string? EmailServerName { get; set; }

        [StringLength(10)]
        public string? EmailPortNo { get; set; }

        public bool IsViewOnly { get; set; } = false;


        // Newley Added Fields
        public Guid? QrToken { get; set; }

        public string? QrCodeImage { get; set; }

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

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool HideManualJobOrderButton { get; set; } = false;

        public List<int> StateCodes { get; set; } = new();

        public string? StateCodesCsv
        {
            get => StateCodes != null && StateCodes.Any() ? string.Join(",", StateCodes) : null;
            set => StateCodes = !string.IsNullOrWhiteSpace(value)
                ? value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                : new List<int>();
        }

        public ICollection<UserRightVM> UserRights { get; set; } = new List<UserRightVM>();

        public bool ChangePassword { get; set; } = false;

        // ----------------- CUSTOM VALIDATION -----------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // --- Password Validation ---
            if (UserId == 0 || ChangePassword)
            {
                if (string.IsNullOrWhiteSpace(UserPassword))
                {
                    yield return new ValidationResult("Password is required.", new[] { nameof(UserPassword) });
                }
                else if (UserPassword.Length < 6)
                {
                    yield return new ValidationResult("Password must be at least 6 characters long.", new[] { nameof(UserPassword) });
                }
                // Password complexity: at least 1 uppercase, 1 lowercase, 1 number, 1 special character
                else if (!Regex.IsMatch(UserPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$"))
                {
                    yield return new ValidationResult("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.", new[] { nameof(UserPassword) });
                }

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult("Confirm Password is required.", new[] { nameof(ConfirmPassword) });
                }
                else if (UserPassword != ConfirmPassword)
                {
                    yield return new ValidationResult("Passwords do not match.", new[] { nameof(ConfirmPassword) });
                }
            }

            // --- Email App Password Validation ---
            bool isEmailIdProvided = !string.IsNullOrWhiteSpace(EmailId);
            bool isEmailAppPasswordAttempted = !string.IsNullOrWhiteSpace(EmailAppPassword) || !string.IsNullOrWhiteSpace(EmailConfirmAppPassword);

            if (isEmailIdProvided && isEmailAppPasswordAttempted)
            {
                if (string.IsNullOrWhiteSpace(EmailAppPassword))
                {
                    yield return new ValidationResult("E-mail App Password is required when Confirm E-mail App Password is provided.", new[] { nameof(EmailAppPassword) });
                }
                if (string.IsNullOrWhiteSpace(EmailConfirmAppPassword))
                {
                    yield return new ValidationResult("Confirm E-mail App Password is required when E-mail App Password is provided.", new[] { nameof(EmailConfirmAppPassword) });
                }
                else if (EmailAppPassword != EmailConfirmAppPassword)
                {
                    yield return new ValidationResult("E-mail App Passwords do not match.", new[] { nameof(EmailConfirmAppPassword) });
                }
            }
        }
    }


}
