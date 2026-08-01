using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.LabourDc_profile
{
    public class LabourDcReturnCompTrackProfile : Profile
    {
        public LabourDcReturnCompTrackProfile()
        {
            // ===================== VM → Entity =====================
            CreateMap<LabourDcReturnCompTrackVM, LabourDcReturnCompTrack>()

                // Ignore navigation properties (EF will manage them)
                .ForMember(dest => dest.LabourDcOutgoingSub, opt => opt.Ignore())
                .ForMember(dest => dest.LabourSCNSub, opt => opt.Ignore())
                .ForMember(dest => dest.LabourGRNSub, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.InComingItem, opt => opt.Ignore())
                .ForMember(dest => dest.OutgoingItem, opt => opt.Ignore());


            // ===================== Entity → VM =====================
            CreateMap<LabourDcReturnCompTrack, LabourDcReturnCompTrackVM>()


                // SCN
                .ForMember(dest => dest.SCNNo,
                    opt => opt.MapFrom(src =>
                        src.LabourSCNSub != null
                            ? src.LabourSCNSub.LabourSCN.SCNNo
                            : null))

                // GRN
                .ForMember(dest => dest.GRNNo,
                    opt => opt.MapFrom(src =>
                        src.LabourGRNSub != null
                            ? src.LabourGRNSub.LabourGRN.GRNNo +
                              src.LabourGRNSub.LabourGRN.Suffix
                            : null))

                // PO
                .ForMember(dest => dest.PoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PONo
                            : null));
        }
    }
}
