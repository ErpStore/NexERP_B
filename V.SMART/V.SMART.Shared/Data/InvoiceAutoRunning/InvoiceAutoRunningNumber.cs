using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.InvoiceAutoRunning
{
    public class InvoiceAutoRunningNumber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceType { get; set; }

        [Required]
        [MaxLength(20)]
        public string Suffix { get; set; }

        [Required]
        public long LastNumber { get; set; }
    }
}
