using AutoMapper;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.AccountsMasterProfile
{
    public class CostCenterMapping : Profile
    {
        public CostCenterMapping()
        {
            // Entity -> VM
            CreateMap<CostCenter, CostCenterVM>()
                .ForMember(dest => dest.ProjectTypeName, opt => opt.MapFrom(src => src.ProjectTypeMaster != null ? src.ProjectTypeMaster.TypeOfProject : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty));

            // VM -> Entity
            CreateMap<CostCenterVM, CostCenter>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTypeMaster, opt => opt.Ignore());
        }
    }
}
