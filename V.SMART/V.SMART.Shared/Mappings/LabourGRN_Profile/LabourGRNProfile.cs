using AutoMapper;
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
    public class LabourGRNProfile:Profile
    {
        public LabourGRNProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<LabourGRN, LabourGRNVM>()
                .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
                .ForMember(dest => dest.CustContactNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))
                .ForMember(dest => dest.CustGst, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))

                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.LabourGRNSubVMs, opt => opt.MapFrom(src => src.LabourGRNSubs));
                

            // ===================== VM -> Entity =====================
            CreateMap<LabourGRNVM, LabourGRN>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore()) // handled separately
                .ForMember(dest => dest.LabourGRNSubs, opt => opt.Ignore());
        }
    }
}
