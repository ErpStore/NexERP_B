using AutoMapper;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.PlanningMappingProfile.JobOrderMappingProfile
{
    public class JobOrderProfile : Profile
    {
        public JobOrderProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<JobOrder, JobOrderVM>()

                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

              .ForMember(dest => dest.DepartmentCode,
                         opt => opt.MapFrom(src => src.Staff != null ? src.Staff.DepartmentCode : string.Empty))

                .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPo != null
                            ? $"{src.MfgPo.PONo}{src.MfgPo.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.RefPODate,
                    opt => opt.MapFrom(src => src.MfgPo != null ? src.MfgPo.PODate : null))

                  .ForMember(dest => dest.LineNo,
                            opt => opt.MapFrom(src =>
                                src.MfgPoSub != null
                                    ? src.MfgPoSub.LineNo.ToString()
                                    : string.Empty))

                .ForMember(dest => dest.LineNo, opt => opt.MapFrom(src => src.MfgPoSub != null  ? src.MfgPoSub.LineNo : 0))

                .ForMember(dest => dest.AssemblyItemCode,
                    opt => opt.MapFrom(src => src.AssyItem != null ? src.AssyItem.ItemCode : string.Empty))

                .ForMember(dest => dest.JobOrderSubVMs,

                    opt => opt.MapFrom(src => src.JobOrderSubs));

            // ===================== VM -> Entity =====================
            CreateMap<JobOrderVM, JobOrder>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPo, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.AssyItem, opt => opt.Ignore())
                .ForMember(dest => dest.JobOrderSubs, opt => opt.Ignore());
        }
    }

}
