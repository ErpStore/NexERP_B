using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchaseGRNMapping
{
    public class PurchaseGRNSubProfile:Profile
    {
        public PurchaseGRNSubProfile()
        {
            CreateMap<PurchaseGRNSub, PurchaseGRNSubVM>()
            // Item details
            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
            .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
            .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
            .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Item != null ? src.Item.Weight : (decimal?)null))


            //Category
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty))

            // PO-related fields

            .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.PurchPoSub != null
                            ? src.PurchPoSub.PurchPo.PONo + "" + src.PurchPoSub.PurchPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.PurchPoSub != null
                            ? src.PurchPoSub.PurchPo.PODate
                            : (DateTime?)null))

                .ForMember(dest => dest.PoDueDate,
                    opt => opt.MapFrom(src =>
                        src.PurchPoSub != null
                            ? src.PurchPoSub.DueDate
                            : (DateTime?)null))

            .ForMember(dest => dest.POBalQty, opt => opt.MapFrom(src => src.PurchPoSub != null ? src.PurchPoSub.BalQty : (decimal?)null))


                .ForMember(dest => dest.InspectNo,
                    opt => opt.MapFrom(src =>
                        src.IncomingInspection != null
                            ? $"{src.IncomingInspection.InspectNo}{src.IncomingInspection.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.InspectDate,
                    opt => opt.MapFrom(src => src.IncomingInspection != null
                        ? src.IncomingInspection.InspectDate
                        : (DateTime?)null));


            CreateMap<PurchaseGRNSubVM, PurchaseGRNSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.PurchPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseGRN, opt => opt.Ignore());
        }
    }
}
