using AutoMapper;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.PurchaseQuotion
{
    public class PurchaseQuoteSubProfile: Profile
    {
        public PurchaseQuoteSubProfile()
        {

            // ===================== Entity -> VM =====================

            CreateMap<PurchaseQuoteSub, PurchaseQuoteSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))


                .ForMember(dest => dest.RefEnqNo,
                    opt => opt.MapFrom(src =>
                        src.EnquiryPurchaseVendorAssign != null
                            ? src.EnquiryPurchaseVendorAssign.Enquiry.EnquiryNo + "" + src.EnquiryPurchaseVendorAssign.Enquiry.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefEnqDate,
                    opt => opt.MapFrom(src =>
                        src.EnquiryPurchaseVendorAssign != null
                            ? src.EnquiryPurchaseVendorAssign.Enquiry.EnquiryDate
                            : (DateTime?)null))


                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));

            // ===================== VM -> Entity =====================

            CreateMap<PurchaseQuoteSubVM, PurchaseQuoteSub>()
                            .ForMember(dest => dest.Item, opt => opt.Ignore())
                            .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                            .ForMember(dest => dest.EnquiryPurchaseVendorAssign, opt => opt.Ignore());
        }
    }
}
