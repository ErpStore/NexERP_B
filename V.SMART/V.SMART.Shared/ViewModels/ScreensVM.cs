using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class ScreensVM
    {
        public int Id { get; set; }
        public int ScreenCode { get; set; }
        public string ScreenName { get; set; }
        public bool IsPrintRequired { get; set; }

        public bool IsLocked { get; set; } // 🔥 ADD THIS
    }
}
