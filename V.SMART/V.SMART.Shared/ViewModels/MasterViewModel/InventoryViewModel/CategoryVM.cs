using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class CategoryVM
    {
        public int CategoryCode { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [MaxLength(100, ErrorMessage = "Category Name must be at most 100 characters.")]
        public string CategoryName { get; set; }


        [MaxLength(250, ErrorMessage = "Category Name must be at most 250 characters.")]
        public string? CategoryDesc { get; set; }

        public bool IsSystemDefined { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
