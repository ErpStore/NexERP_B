using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ExpInv_VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ExpInv_Profile
{
    public class ExpInvProfile:Profile
    {
        public ExpInvProfile()
        {
            CreateMap<ExpInv, ExpInvVM>()
           .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
           .ForMember(dest => dest.CustAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
           .ForMember(dest => dest.GSTNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))
           .ForMember(dest => dest.ContactNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))

           // Consignee / Shipping
           .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.AltCustName : string.Empty))
           .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Consinee != null ?
               string.Join(" ", new[] { src.Consinee.AltCustAddr1, src.Consinee.AltCustAddr2 }.Where(a => !string.IsNullOrEmpty(a)))
               : string.Empty))
           .ForMember(dest => dest.ShippingGstin, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.GSTNo : string.Empty))
           .ForMember(dest => dest.ShippingContactNo, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.ContactNo : string.Empty))
           .ForMember(dest => dest.StoreIssueName, opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : string.Empty))

           // Currency
           .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))
           .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))

           .ForMember(dest => dest.ExpSubInvVMs, opt => opt.MapFrom(src => src.ExpInvSubs));

            // ===================== VM -> Entity =====================
            CreateMap<ExpInvVM, ExpInv>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Consinee, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.StoreIssue, opt => opt.Ignore())
                .ForMember(dest => dest.ExpInvSubs, opt => opt.Ignore());
        }
    }
}
