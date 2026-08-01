using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.GeneralSettingsMaster
{
    public class PrintSetting
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select screen name. Screen name should not be empty.")]
        [StringLength(50)]
        public string ScreenName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter CopyName. CopyName should not be empty.")]
        [StringLength(50)]
        public string CopyName { get; set; } = "Original";

        public int NoOfCopies { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public bool WatermarkRequired { get; set; } = false;
        public bool LogoRequired { get; set; } = false;
        public bool IsoLogoRequired { get; set; } = false;

        public string? WatermarkLable { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [NotMapped]
        public bool IsEditable { get; set; } = false;

    }

}
