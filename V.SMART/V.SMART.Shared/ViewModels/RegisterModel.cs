using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace V.SMART.Shared.ViewModels
{

    public class RegisterModel : IValidatableObject
    {
        public int UserId { get; set; } = 0;

        public int? StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff? Staff { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [MinLength(3, ErrorMessage = "User Name must be at least 3 characters long")]
        public string? UserName { get; set; }

        [DataType(DataType.Password)]
        public string? UserPassword { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? PhoneNumber { get; set; }

        public bool LevelAuthorization { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? EmailId { get; set; }

        [DataType(DataType.Password)]
        public string? EmailAppPassword { get; set; }

        [DataType(DataType.Password)]
        public string? EmailConfirmAppPassword { get; set; }

        public string? EmailServerName { get; set; }

        public string? EmailPortNo { get; set; }

        public List<int> StateCodes { get; set; } = new();

        [Required(ErrorMessage = "User role is required.")]
        public UserRole Role { get; set; } = UserRole.User;

        public bool ChangePassword { get; set; } = false;
        public bool IsViewOnly { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // --- User Password Validation ---
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
                // Regex for password complexity (at least one uppercase, one lowercase, one number, one special character)
                else if (!System.Text.RegularExpressions.Regex.IsMatch(UserPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$"))
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
            bool isEmailIdProvided = !string.IsNullOrWhiteSpace(EmailId);
            bool isEmailAppPasswordAttempted = !string.IsNullOrWhiteSpace(EmailAppPassword) || !string.IsNullOrWhiteSpace(EmailConfirmAppPassword);

            if (isEmailIdProvided)
            {
                if (isEmailAppPasswordAttempted)
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

}