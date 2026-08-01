using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel
{
    public class UserRightVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public int UserId { get; set; }

        public string? UserName { get; set; }


        [Required(ErrorMessage = "Screen Name is required.")]
        public int ScreenCode { get; set; }

        public string? ScreenName { get; set; } // Optional display name for UI

        public bool CanView { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool IsHide { get; set; } = false;

        public DateTime? CreatedDate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
    }
}
