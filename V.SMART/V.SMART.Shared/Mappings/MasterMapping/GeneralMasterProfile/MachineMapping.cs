using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.GeneralMasterProfile
{
    public class MachineMapping : Profile
    {
        public MachineMapping()
        {
            // Entity -> VM
            CreateMap<Machine, MachineVM>();

            // VM -> Entity
            CreateMap<MachineVM, Machine>();
        }
    }
}
