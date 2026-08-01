using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchaseInvoice_Mapping
{
    public class PurchaseInvoiceSubProfile : Profile
    {
        public PurchaseInvoiceSubProfile()
        {
            CreateMap<PurchaseInvoiceSub, PurchaseInvoiceSubVM>()

            .ForMember(dest => dest.RefSCNNo, opt => opt.MapFrom(src => src.PurchaseSCNSub.PurchaseSCN != null ? src.PurchaseSCNSub.PurchaseSCN.SCNNo + "" + src.PurchaseSCNSub.PurchaseSCN.Suffix : string.Empty))
            .ForMember(dest => dest.RefSCNDate, opt => opt.MapFrom(src => src.PurchaseSCNSub.PurchaseSCN != null ? (DateTime?)src.PurchaseSCNSub.PurchaseSCN.SCNDate : null))
            
            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
            .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
            .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
            .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Item != null ? src.Item.Weight : (decimal?)null))


            //Category
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty))

            .ForMember(dest => dest.DebititNos,
                 opt => opt.MapFrom(src => src.DebitNoteSubs
                .Where(x => x.RefPurchInvSubId == src.InvSubId)
                .Select(x => x.DebitNote.DebitNo + x.DebitNote.Suffix)
                .Distinct()
                .ToList()))

            .ForMember(dest => dest.DebititDates,
                    opt => opt.MapFrom(src => src.DebitNoteSubs
                        .Where(x => x.RefPurchInvSubId == src.InvSubId)
                        .Select(x => x.DebitNote.DebitDate)
                        .Distinct()
                        .ToList()));


            CreateMap<PurchaseInvoiceSubVM, PurchaseInvoiceSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseSCNSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseInvoice, opt => opt.Ignore());
        }
    }
}
