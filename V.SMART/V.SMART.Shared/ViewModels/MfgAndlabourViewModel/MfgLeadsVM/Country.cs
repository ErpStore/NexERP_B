using V.SMART.Shared.Data.Master.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgLeadsVM
{
    public class Country
    {
        public string Name { get; set; }
        public List<State> States { get; set; }
    }
}
