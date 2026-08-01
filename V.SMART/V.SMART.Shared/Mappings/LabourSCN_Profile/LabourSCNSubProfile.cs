using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.LabourSCN_Profile
{
    public class LabourSCNSubProfile:Profile
    {
        public LabourSCNSubProfile()
        {
            CreateMap<LabourSCNSub, LabourSCNSubVM>()
            // Item details

            .ForMember(dest => dest.RefGRNNo, opt => opt.MapFrom(src => src.LabourGRNSub.LabourGRN != null ? src.LabourGRNSub.LabourGRN.GRNNo + "" + src.LabourGRNSub.LabourGRN.Suffix : string.Empty))
            .ForMember(dest => dest.RefGRNDate, opt => opt.MapFrom(src => src.LabourGRNSub.LabourGRN != null ? (DateTime?)src.LabourGRNSub.LabourGRN.GRNDate : null))

            .ForMember(dest => dest.RefDcNo, opt => opt.MapFrom(src => src.LabourGRNSub.LabourGRN != null ? src.LabourGRNSub.LabourGRN.RefDcNo : string.Empty))
            .ForMember(dest => dest.RefDcDate, opt => opt.MapFrom(src => src.LabourGRNSub.LabourGRN != null ? (DateTime?)src.LabourGRNSub.LabourGRN.RefDcDate : null))

            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
            .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
            .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Item != null ? src.Item.Weight : (decimal?)null))
            .ForMember(dest => dest.UnitConvert, opt => opt.MapFrom(src => src.Item != null ? src.Item.UnitConvert : (decimal?)null))

            //Item Specification From Labour GRN
            .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.LabourGRNSub != null ? src.LabourGRNSub.ItemSpecification : string.Empty))

            //Category
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty));

            CreateMap<LabourSCNSubVM, LabourSCNSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.LabourGRNSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.LabourSCN, opt => opt.Ignore());
        }
    }
}
