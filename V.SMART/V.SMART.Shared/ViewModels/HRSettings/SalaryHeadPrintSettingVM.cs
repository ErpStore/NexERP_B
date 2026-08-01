using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IQSMART.Shared.ViewModels.Settings
{
    public class SalaryHeadPrintSettingVM
    {
        public int Id { get; set; }

        [Display(Name = "Field Name")]
        public string? FieldName { get; set; }

        [Required(ErrorMessage = "Screen Name is required")]
        [Display(Name = "Name In Screen")]
        public string? ScrColName { get; set; }

        [Required(ErrorMessage = "Print Name is required")]
        [StringLength(20, ErrorMessage = "Maximum 20 characters allowed")]
        [Display(Name = "Printed As")]
        public string? PrintName { get; set; }

        [Display(Name = "Print")]
        public bool IsPrint { get; set; }

        public int Type { get; set; }

        public int? StrRow { get; set; }

        public int? StrCol { get; set; }

        public int? VertCol { get; set; }

        public int? VertRow { get; set; }
    }
}
