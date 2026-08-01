using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM
{
    public class EnquirySalesVM
    {

        public int EnquiryId { get; set; }

        [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters.")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Enquiry No. is required.")]
        [StringLength(30, ErrorMessage = "Enquiry No. cannot exceed 30 characters.")]
        public string EnquiryNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Enquiry Date is required.")]
        public DateTime EnquiryDate { get; set; } = DateTime.Now;
        
        public DateTime EnquiryDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerPhoneNo { get; set; }
        public string CustomerGSTNO { get; set; }


        [Range(0, int.MaxValue, ErrorMessage = "No of items must be a valid number.")]
        public int NoOfItems => EnquirySalesSubVM?.Count ?? 0;

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? MainRemarks { get; set; }

        [StringLength(80, ErrorMessage = "Kind Of Attention cannot exceed 80 characters.")]
        public string? KindOfAttention { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total value must be a positive number.")]
        public decimal TotalValue { get; set; }

        public bool EnquiryTally { get; set; }

        [Required(ErrorMessage = "please select either Manufacturing or Labour")]
        public bool MfgORLab { get; set; } = true;

        public bool Cancel { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public bool ShortClose { get; set; } = false;

        public string? POCName { get; set; }

        [Phone(ErrorMessage = "Invalid contact number.")]
        public string? POCContactNo { get; set; }

        [DateGreaterThan("EnquiryDate", ErrorMessage = "Expected Reply Date must be greater than Enquiry Date.")]
        public DateTime? ExpectedReplyDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }



        [MinLength(1, ErrorMessage = "At least one Enquiry item is required.")]
        [ValidateComplexType]
        public List<EnquirySalesSubVM> EnquirySalesSubVM { get; set; } = new();


        // ===================== Custom Validation Attribute =====================
        public class DateGreaterThanAttribute : ValidationAttribute
        {
            private readonly string _comparisonProperty;

            public DateGreaterThanAttribute(string comparisonProperty)
            {
                _comparisonProperty = comparisonProperty;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var currentValue = value as DateTime?;
                var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

                if (comparisonProperty == null)
                    throw new ArgumentException($"Property {_comparisonProperty} not found.");

                var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance) as DateTime?;

                if (currentValue.HasValue && comparisonValue.HasValue)
                {
                    if (currentValue <= comparisonValue)
                    {
                        return new ValidationResult(ErrorMessage ??
                            $"{validationContext.DisplayName} must be greater than {_comparisonProperty}.");
                    }
                }

                return ValidationResult.Success;
            }
        }

    }

}
