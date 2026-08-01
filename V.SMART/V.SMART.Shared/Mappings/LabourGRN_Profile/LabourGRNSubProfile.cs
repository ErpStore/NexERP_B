using AutoMapper;
using FastReport.Utils;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.LabourGRN_Profile
{
    public class LabourGRNSubProfile:Profile
    {
        public LabourGRNSubProfile() 
        {

            // ===================== Entity -> VM =====================

            CreateMap<LabourGRNSub, LabourGRNSubVM>()

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty))
                .ForMember(dest => dest.CategoryCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryCode : 0))
                .ForMember(dest => dest.LabourCost, opt => opt.MapFrom(src => src.Item!.AssemblyPrice))
                .ForMember(dest => dest.RefPoNo, opt => opt.MapFrom(src => src.MfgPoSub.MfgPo != null ? src.MfgPoSub.MfgPo.PONo : string.Empty))
                .ForMember(dest => dest.RefPoDate,opt => opt.MapFrom(src => src.MfgPoSub != null && src.MfgPoSub.MfgPo != null ? src.MfgPoSub.MfgPo.PODate: (DateTime?)null))
                .ForMember(dest => dest.POBalQty, opt => opt.MapFrom(src => src.MfgPoSub.BalQty))

                .ForMember(dest => dest.ItemSpecification, opt => opt.MapFrom(src => src.MfgPoSub != null ? src.MfgPoSub.ItemSpecification : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));


            // ===================== VM -> Entity =====================

            CreateMap<LabourGRNSubVM, LabourGRNSub>()
                            .ForMember(dest => dest.Item, opt => opt.Ignore())
                            .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                            .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                            .ForMember(dest => dest.LabourGRN, opt => opt.Ignore());
        }
    }
}
