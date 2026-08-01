using V.SMART.Shared.Data.Master.Admin;
using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.MasterScreeenManagement
{
    public class Screens
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScreenCode { get; set; }

        [Required]
        public string ScreenName { get; set; }
        public bool IsPrintRequired { get; set; }

        public ICollection<UserRight> UserRights { get; set; }

    }
}
