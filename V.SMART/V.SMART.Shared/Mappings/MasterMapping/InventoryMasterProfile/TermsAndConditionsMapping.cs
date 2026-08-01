using AutoMapper;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.InventoryMasterProfile
{
    public class TermsAndConditionsMapping : Profile
    {
        public TermsAndConditionsMapping()
        {
            // Entity -> VM
            CreateMap<TermsAndConditions, TermsAndConditionsVM>();

            // VM -> Entity
            CreateMap<TermsAndConditionsVM, TermsAndConditions>();
        }
    }
}
