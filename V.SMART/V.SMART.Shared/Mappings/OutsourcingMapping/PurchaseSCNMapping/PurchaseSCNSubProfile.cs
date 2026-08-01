using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchaseSCNMapping
{
    public class PurchaseSCNSubProfile:Profile
    {
        public PurchaseSCNSubProfile()
        {
            CreateMap<PurchaseSCNSub, PurchaseSCNSubVM>()
            // Item details

            .ForMember(dest => dest.RefGRNNo, opt => opt.MapFrom(src => src.PurchaseGRNSub.PurchaseGRN != null ? src.PurchaseGRNSub.PurchaseGRN.GRNNo + "" + src.PurchaseGRNSub.PurchaseGRN.Suffix : string.Empty))
            .ForMember(dest => dest.RefGRNDate, opt => opt.MapFrom(src => src.PurchaseGRNSub.PurchaseGRN != null ? (DateTime?)src.PurchaseGRNSub.PurchaseGRN.GRNDate : null))

            .ForMember(dest => dest.RefDcNo, opt => opt.MapFrom(src => src.PurchaseGRNSub.PurchaseGRN != null ? src.PurchaseGRNSub.PurchaseGRN.RefDcNo : string.Empty))
            .ForMember(dest => dest.RefDcDate, opt => opt.MapFrom(src => src.PurchaseGRNSub.PurchaseGRN != null ? (DateTime?)src.PurchaseGRNSub.PurchaseGRN.RefDcDate : null))

            .ForMember(dest => dest.RefInvNo, opt => opt.MapFrom(src => src.PurchaseGRNSub.PurchaseGRN != null ? src.PurchaseGRNSub.PurchaseGRN.RefInvNo : string.Empty))

            .ForMember(dest => dest.RefInvDate, opt => opt.MapFrom(src =>src.PurchaseGRNSub != null && src.PurchaseGRNSub.PurchaseGRN != null 
             && !string.IsNullOrEmpty(src.PurchaseGRNSub.PurchaseGRN.RefInvNo) ? (DateTime?)src.PurchaseGRNSub.PurchaseGRN.RefInvDate : null ))

            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
            .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
            .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
            .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Item != null ? src.Item.Weight : (decimal?)null))
            .ForMember(dest => dest.UnitConvert, opt => opt.MapFrom(src => src.Item != null ? src.Item.UnitConvert : (decimal?)null))

            //Category
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty));


            CreateMap<PurchaseSCNSubVM, PurchaseSCNSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseGRNSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseSCN, opt => opt.Ignore());
        }
    }
}
