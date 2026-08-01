using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.LabourDc_profile
{
    public class LabourDcOutgoingSubProfile : Profile
    {
        public LabourDcOutgoingSubProfile()
        {
            // ===================== Entity -> VM =====================

            CreateMap<LabourDcOutgoingSub, LabourDcOutgoingSubVM>()

                .ForMember(dest => dest.ItemCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))

                .ForMember(dest => dest.ItemName,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))

                .ForMember(dest => dest.HSNCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))

                .ForMember(dest => dest.MeasureUnit,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.Specification,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))

                .ForMember(dest => dest.Category,
                    opt => opt.MapFrom(src => src.Item != null && src.Item.Category != null
                        ? src.Item.Category.CategoryName
                        : string.Empty))

                .ForMember(dest => dest.CategoryCode,
                    opt => opt.MapFrom(src => src.Item != null && src.Item.Category != null
                        ? src.Item.Category.CategoryCode
                        : 0))

                // PO
                .ForMember(d => d.PoId,
                    o => o.MapFrom(s => s.MfgPoSub != null ? s.MfgPoSub.MfgPo.PoId : (int?)null))

                .ForMember(d => d.RefPoSubId,
                    o => o.MapFrom(s => s.MfgPoSub != null ? s.MfgPoSub.PoSubId : (int?)null))

                .ForMember(d => d.RefPoNo,
                    o => o.MapFrom(s => s.MfgPoSub != null ? s.MfgPoSub.MfgPo.PONo : string.Empty))

                .ForMember(d => d.RefPoDate,
                    o => o.MapFrom(s => s.MfgPoSub != null ? s.MfgPoSub.MfgPo.PODate : (DateTime?)null))

                .ForMember(dest => dest.LineNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.LineNo
                            : 0))


                // SCN
                .ForMember(d => d.RefSCNNo,
                    o => o.MapFrom(s => s.LabourSCNSub != null
                        ? s.LabourSCNSub.LabourSCN.SCNNo
                        : string.Empty))

                .ForMember(d => d.RefSCNDate,
                    o => o.MapFrom(s => s.LabourSCNSub != null
                        ? s.LabourSCNSub.LabourSCN.SCNDate
                        : (DateTime?)null))

                // GRN
                .ForMember(d => d.RefGRNNo,
                    o => o.MapFrom(s => s.LabourGRNSub != null
                        ? s.LabourGRNSub.LabourGRN.GRNNo + s.LabourGRNSub.LabourGRN.Suffix
                        : string.Empty))

                .ForMember(d => d.RefGRNDate,
                    o => o.MapFrom(s => s.LabourGRNSub != null
                        ? s.LabourGRNSub.LabourGRN.GRNDate
                        : (DateTime?)null))

                .ForMember(d => d.RefDcNo,
                    o => o.MapFrom(s => s.LabourGRNSub != null
                        ? s.LabourGRNSub.LabourGRN.RefDcNo
                        : string.Empty))

                .ForMember(d => d.RefDcDate,
                    o => o.MapFrom(s => s.LabourGRNSub != null
                        ? s.LabourGRNSub.LabourGRN.RefDcDate
                        : (DateTime?)null))


                // List Of DcNos
                .ForMember(d => d.RefDcNos,
                    o => o.MapFrom(s =>
                        s.LabourDcReturnCompTracks != null
                        ? s.LabourDcReturnCompTracks
                            .Where(x => x.LabourGRNSub != null && x.LabourGRNSub.LabourGRN != null)
                            .Select(x => x.LabourGRNSub.LabourGRN.RefDcNo ?? "")
                            .Distinct()
                            .ToList()
                        : new List<string>()))

                // List Of DcDates
                .ForMember(d => d.RefDCDates,
                    o => o.MapFrom(s =>
                        s.LabourDcReturnCompTracks != null
                        ? s.LabourDcReturnCompTracks
                            .Where(x => x.LabourGRNSub != null && x.LabourGRNSub.LabourGRN != null)
                            .Select(x => x.LabourGRNSub.LabourGRN.RefDcDate.HasValue
                                ? x.LabourGRNSub.LabourGRN.RefDcDate.Value.ToString("dd/MM/yyyy")
                                : "")
                            .Distinct()
                            .ToList()
                        : new List<string>()))


                // Cost Center
                .ForMember(dest => dest.ProjectNo,
                    opt => opt.MapFrom(src => src.CostCenter != null
                        ? src.CostCenter.ProjectNo
                        : string.Empty));



            // ===================== VM -> Entity =====================

            CreateMap<LabourDcOutgoingSubVM, LabourDcOutgoingSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.LabourGRNSub, opt => opt.Ignore())
                .ForMember(dest => dest.LabourSCNSub, opt => opt.Ignore())
                .ForMember(dest => dest.LabourDcOutgoing, opt => opt.Ignore());
        }
    }
}
