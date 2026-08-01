using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MfgQuotation
{
    public class LabInvSubProfile : Profile
    {
        public LabInvSubProfile()
        {

            // ===================== Entity -> VM =====================

            CreateMap<LabInvSub, LabInvSubVM>()


              .ForMember(dest => dest.RefDcNo,
                    opt => opt.MapFrom(src =>
                        src.LabourDcOutgoingSub != null &&
                        src.LabourDcOutgoingSub.LabourDcOutgoing != null
                            ? src.LabourDcOutgoingSub.LabourDcOutgoing.DcNo +
                              src.LabourDcOutgoingSub.LabourDcOutgoing.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefDcDate,
                    opt => opt.MapFrom(src =>
                        src.LabourDcOutgoingSub != null &&
                        src.LabourDcOutgoingSub.LabourDcOutgoing != null
                            ? src.LabourDcOutgoingSub.LabourDcOutgoing.DcDate
                            : (DateTime?)null))

                .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.LabourDcOutgoingSub != null &&
                        src.LabourDcOutgoingSub.MfgPoSub != null &&
                        src.LabourDcOutgoingSub.MfgPoSub.MfgPo != null
                            ? src.LabourDcOutgoingSub.MfgPoSub.MfgPo.PONo
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.LabourDcOutgoingSub != null &&
                        src.LabourDcOutgoingSub.MfgPoSub != null &&
                        src.LabourDcOutgoingSub.MfgPoSub.MfgPo != null
                            ? src.LabourDcOutgoingSub.MfgPoSub.MfgPo.PODate
                            : (DateTime?)null))

                .ForMember(dest => dest.LineNo,
                    opt => opt.MapFrom(src =>
                        src.LabourDcOutgoingSub != null &&
                        src.LabourDcOutgoingSub.MfgPoSub != null
                            ? src.LabourDcOutgoingSub.MfgPoSub.LineNo
                            : 0))


                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.SACCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.SACCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.LabourCost, opt => opt.MapFrom(src => src.Item!.AssemblyPrice))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

             .ForMember(dest => dest.CreditNos, opt => opt.MapFrom(src => src.CreditNoteSubs
                                .Select(x => x.CreditNote.CreditNo + x.CreditNote.Suffix)
                                .Distinct().ToList()))

                    .ForMember(dest => dest.CreditDates,
                        opt => opt.MapFrom(src => src.CreditNoteSubs.Select(x => x.CreditNote.CreditDate)
                                .Distinct().ToList()));

            // ===================== VM -> Entity =====================

            CreateMap<LabInvSubVM, LabInvSub>()
                            .ForMember(dest => dest.Item, opt => opt.Ignore())
                            .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                            .ForMember(dest => dest.CreditNoteSubs, opt => opt.Ignore())
                            .ForMember(dest => dest.LabourDcOutgoingSub, opt => opt.Ignore());
        }
    }
}
