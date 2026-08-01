using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchOrSubComEnquiryMapping
{
    public class EnquiryPurchaseSubProfile : Profile
    {
        public EnquiryPurchaseSubProfile()
        {
            // ------------------------------------------------------------
            // ENTITY → VIEWMODEL
            // ------------------------------------------------------------
            CreateMap<EnquiryPurchaseSub, EnquiryPurchaseSubVM>()

                .ForMember(dest => dest.RefMReqNo,
                    opt => opt.MapFrom(src =>
                        src.MaterialReqSub != null
                            ? src.MaterialReqSub.MaterialReq.MReqNo + "" + src.MaterialReqSub.MaterialReq.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefMReqDate,
                    opt => opt.MapFrom(src =>
                        src.MaterialReqSub != null
                            ? src.MaterialReqSub.MaterialReq.MreqDate
                            : (DateTime?)null))

                .ForMember(dest => dest.ItemCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : null))
                .ForMember(dest => dest.ItemName,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : null))
                .ForMember(dest => dest.HsnCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : null))
                .ForMember(dest => dest.UOM,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : null))
                .ForMember(dest => dest.ProjectNo,
                    opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.CostCenterName : null))
                .ForMember(dest => dest.SelectedItem, opt => opt.Ignore())
                .ForMember(dest => dest.IsEditable, opt => opt.Ignore());

            // ------------------------------------------------------------
            // VIEWMODEL → ENTITY
            // ------------------------------------------------------------
            CreateMap<EnquiryPurchaseSubVM, EnquiryPurchaseSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore()) 
                .ForMember(dest => dest.EnquiryPurchase, opt => opt.Ignore()) 
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate,
                    opt => opt.MapFrom(src => src.DueDate == default(DateTime) ? (DateTime?)null : src.DueDate));
        }
    }
}
