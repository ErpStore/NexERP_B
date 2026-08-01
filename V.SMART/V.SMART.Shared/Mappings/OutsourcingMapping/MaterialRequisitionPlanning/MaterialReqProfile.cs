using AutoMapper;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.MaterialRequisitionPlanning
{
    public class MaterialReqProfile : Profile
    {
        public MaterialReqProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<MaterialReq, MaterialReqVM>()
                

                // Sub Items (Child Collection)
                .ForMember(dest => dest.MaterialReqSubVMs, opt => opt.MapFrom(src => src.MaterialReqSubs));

            // ===================== VM -> Entity =====================
            CreateMap<MaterialReqVM, MaterialReq>()
                // Ignore navigation properties (we map only foreign keys)
                .ForMember(dest => dest.MaterialReqSubs, opt => opt.Ignore());
        }
    }
}
