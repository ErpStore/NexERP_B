using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Admin
{
    public class UserRight
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User is required.")]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int ScreenId { get; set; }

        [ForeignKey(nameof(ScreenId))]
        public Screens Screens { get; set; }


        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool IsHide { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(100, ErrorMessage = "CreatedBy cannot exceed 100 characters.")]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100, ErrorMessage = "UpdatedBy cannot exceed 100 characters.")]
        public string? UpdatedBy { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }
    }

}
