using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.SubContractInvoice;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubContractInvoiceMapping
{
    public class SubConInvSubProfile : Profile
    {
        public SubConInvSubProfile()
        {
            CreateMap<SubConInvSub, SubConInvSubVM>()

            .ForMember(dest => dest.RefSCNNo, opt => opt.MapFrom(src =>
                    src.SubConSCNSub.SubConSCN != null
                    ? src.SubConSCNSub.SubConSCN.SCNNo + "" + src.SubConSCNSub.SubConSCN.Suffix : string.Empty))

            .ForMember(dest => dest.RefSCNDate, opt => opt.MapFrom(src =>
                    src.SubConSCNSub.SubConSCN != null ? (DateTime?)src.SubConSCNSub.SubConSCN.SCNDate : null))



            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
            .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
            .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
            .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

            .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))

            .ForMember(dest => dest.DebititNos,
                    opt => opt.MapFrom(src => src.DebitNoteSubs
                            .Where(x => x.RefSubConInvSubId == src.InvSubId)
                            .Select(x => x.DebitNote.DebitNo + x.DebitNote.Suffix)
                            .Distinct()
                            .ToList()))

            .ForMember(dest => dest.DebititDates,
                    opt => opt.MapFrom(src => src.DebitNoteSubs
                        .Where(x => x.RefSubConInvSubId == src.InvSubId)
                        .Select(x => x.DebitNote.DebitDate)
                        .Distinct()
                        .ToList()));

            CreateMap<SubConInvSubVM, SubConInvSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.SubConInv, opt => opt.Ignore())
                .ForMember(dest => dest.SubConSCNSub, opt => opt.Ignore());
        }
    }
}
