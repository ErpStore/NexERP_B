using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.LabourDc_profile
{
    public class LabourDcOutgoingProfile : Profile
    {
        public LabourDcOutgoingProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<LabourDcOutgoing, LabourDcOutgoingVM>()

                // -------- Customer --------
                .ForMember(dest => dest.CustName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

                .ForMember(dest => dest.CustAddress,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))

                .ForMember(dest => dest.ContactNo,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))

                .ForMember(dest => dest.CustGstNo,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))


                // -------- Consignee / Shipping --------
                .ForMember(dest => dest.ShippingName,
                    opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.AltCustName : string.Empty))

                .ForMember(dest => dest.ShippingAddress,
                    opt => opt.MapFrom(src => src.Consignee != null
                        ? string.Join(" ",
                            new[]
                            {
                            src.Consignee.AltCustAddr1,
                            src.Consignee.AltCustAddr2
                            }.Where(a => !string.IsNullOrWhiteSpace(a)))
                        : string.Empty))

                .ForMember(dest => dest.ShippingGstNo,
                    opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.GSTNo : string.Empty))

                .ForMember(dest => dest.ShippingPhone,
                    opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.ContactNo : string.Empty))


                // -------- Issue Store --------
                .ForMember(dest => dest.StoreName,
                    opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))


                // -------- Child Items --------
                .ForMember(dest => dest.LabourDcOutgoingSubVMs,
                    opt => opt.MapFrom(src => src.LabourDcOutgoingSubs));


            // ===================== VM -> Entity =====================
            CreateMap<LabourDcOutgoingVM, LabourDcOutgoing>()

                // Ignore navigation properties (handled manually / by EF)
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Consignee, opt => opt.Ignore())
                .ForMember(dest => dest.Store, opt => opt.Ignore())

                // Ignore child collection (handled in service logic)
                .ForMember(dest => dest.LabourDcOutgoingSubs, opt => opt.Ignore())

                // Ignore return tracking collection
                .ForMember(dest => dest.LabourDcReturnCompTracks, opt => opt.Ignore());
        }
    }
}
