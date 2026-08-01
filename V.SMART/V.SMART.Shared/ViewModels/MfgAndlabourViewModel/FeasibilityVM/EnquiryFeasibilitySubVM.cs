using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM
{
    public class EnquiryFeasibilitySubVM
    {
        public int FesSubId { get; set; }

        [Required(ErrorMessage = "Feasibility Id is required.")]
        public int FesId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        public ItemVM? SelectedItem { get; set; }

        [Required(ErrorMessage = "Item-Code is Required!..")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Item selected.")]
        public int? ItemId { get; set; }

        [StringLength(250, ErrorMessage = "Item Code cannot exceed 250 characters.")]
        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        public string? HSNCode { get; set; }
        public string? UOM { get; set; }

        [StringLength(250, ErrorMessage = "Remark should not exceed 250 characters.")]
        public string? Remarks { get; set; }

        public string? TypeOfProject { get; set; }

        public string? BaseMaterial { get; set; }

        public string? ToolRequired { get; set; }

        public string? SpclReqOutsource { get; set; }

        public string? TechCapaBilities { get; set; }

        public string? DelvryTrms { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Man Hr Requirement must be greater than zero.")]
        public decimal ManHrReq { get; set; }

        public string? RiskIfAny { get; set; }

        public string? SpclReqCustomer { get; set; }

        public string? CompbltyConfm { get; set; }

        public string? StockAvail { get; set; }

        public decimal IsEditable { get; set; }

    }

}
