using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.Admin_Module
{
    public class UserColumnPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserName { get; set; } = string.Empty;

        [Required]
        public string? ScreenName { get; set; } = string.Empty;

        [Required]
        public string? ColumnJson { get; set; } = string.Empty;

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
