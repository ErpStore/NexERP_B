using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class ItemCustomerAssignVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer selection is required.")]
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }


        [Required(ErrorMessage = "Item selection is required.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }

    }
}
