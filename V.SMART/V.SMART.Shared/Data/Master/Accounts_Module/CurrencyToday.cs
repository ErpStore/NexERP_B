using V.SMART.Shared.Data.Master.Accounts;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Accounts_Module
{
    public class CurrencyToday
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(CurrId))]
        public Currency Currency { get; set; }
        [Required(ErrorMessage = "Please select currency")]
        public int? CurrId { get; set; }

        [Precision(18, 2)]
        [Required(ErrorMessage = "Currency value is required")]
        [Range(0.01, (double)Decimal.MaxValue, ErrorMessage = "Currency value should be greater then zero.")]
        public decimal CurrValueInRs { get; set; }
        public DateTime CurrValueDate { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
