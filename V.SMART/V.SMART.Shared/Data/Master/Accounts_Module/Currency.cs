
using V.SMART.Shared.Data.Master.Accounts_Module;
using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Accounts
{
    public class Currency
    {
        [Key]
        public int CurrId { get; set; }

        [Required(ErrorMessage = "Please Enter Currency Name")]
        public string? CurrName { get; set; }

        [Required(ErrorMessage = "Please Enter PatCurrencyName")]
        public string? CurrSub { get; set; }

        [Required(ErrorMessage = "Please Enter Symbol")]
        [RegularExpression(@"[$€₹¥£₩₿₽]", ErrorMessage = "Only valid currency symbols are allowed (₹, $, €, £, ¥, ₩, ₿, ₽)")]
        public string? Symbol { get; set; }

        public bool IsSystemDefined { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ICollection<CurrencyToday> CurrencyRates { get; set; } = new List<CurrencyToday>();
    }
}
