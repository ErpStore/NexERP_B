using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.MfgQuotation
{
    public class MfgQuoteSubProfile : Profile
    {
        public MfgQuoteSubProfile()
        {

            // ===================== Entity -> VM =====================

            CreateMap<MfgQuoteSub, MfgQuoteSubVM>()

                 .ForMember(dest => dest.RefEnqNo,
                    opt => opt.MapFrom(src =>
                        src.EnquirySalesSub != null
                            ? src.EnquirySalesSub.EnquirySales.EnquiryNo + "" + src.EnquirySalesSub.EnquirySales.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefEnqDate,
                    opt => opt.MapFrom(src =>
                        src.EnquirySalesSub != null
                            ? src.EnquirySalesSub.EnquirySales.EnquiryDate
                            : (DateTime?)null))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.LabourCost, opt => opt.MapFrom(src => src.Item!.AssemblyPrice))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));


            // ===================== VM -> Entity =====================

            CreateMap<MfgQuoteSubVM, MfgQuoteSub>()
                            .ForMember(dest => dest.Item, opt => opt.Ignore())
                            .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                            .ForMember(dest => dest.EnquirySalesSub, opt => opt.Ignore());
        }
    }
}
