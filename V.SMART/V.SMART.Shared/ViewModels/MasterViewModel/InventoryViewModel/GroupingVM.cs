using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class GroupingVM
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "FRC Number is required.")]
        public string FRCNo { get; set; }

        public string? Prefix { get; set; }

        [Range(1, 365)]
        public int LeadTime { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<GroupingSubVM> GroupingSubVMs { get; set; } = new();
    }
}
