using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class StoreVM
    {
        public int StoreId { get; set; }

        [Required(ErrorMessage = "Store Name is required.")]
        [MaxLength(250, ErrorMessage = "Store Name must be at most 250 characters.")]
        public string StoreName { get; set; }

        [MaxLength(250, ErrorMessage = "Store Description must be at most 250 characters.")]
        public string? StoreDesc { get; set; }

        public bool CanIssue { get; set; } = false;

        public bool CanAdd { get; set; } = true;

        public bool IsSystemDefined { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
