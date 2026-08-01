using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.HumanResourceViewModel.AppointmentLetterVM
{
    public class AppointmentLetterSubVM
    {
        public int AppointmentSubId { get; set; }

        public int AppointmentId { get; set; }

        public int SlNo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsEditable { get; set; } = false;
    }
}
