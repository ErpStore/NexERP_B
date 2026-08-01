using AutoMapper;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.HumanResourceMasterProfile
{
    public class ShiftAllocationMapping : Profile
    {

        public ShiftAllocationMapping()
        {
            // ===============================
            // Entity -> ViewModel
            // ===============================
            CreateMap<ShiftAllocation, ShiftAllocationVM>();

            // ===============================
            // ViewModel -> Entity
            // ===============================
            CreateMap<ShiftAllocationVM, ShiftAllocation>();
                   
        }
        

    }
}
