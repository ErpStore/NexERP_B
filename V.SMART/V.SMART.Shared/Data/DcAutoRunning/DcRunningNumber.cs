using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.DcAutoRunning
{
    public class DcRunningNumber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string DcType { get; set; }

        [Required]
        [MaxLength(20)]
        public string Suffix { get; set; }

        [Required]
        public long LastNumber { get; set; }
    }
}
