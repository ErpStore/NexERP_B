using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Admin;

namespace V.SMART.Shared.Data.Master.MasterScreeenManagement_Module
{
    public class UserThemePreference
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public int UserId { get; set; }
        public bool IsDarkMode { get; set; } = false;
    }
}
