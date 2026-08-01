using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ManufacturingDcProfile
{
    public class MfgDcSubProfile : Profile
    {
        public MfgDcSubProfile()
        {
            // ================== Entity -> VM ==================
            CreateMap<MfgDcSub, MfgDcSubVM>()
                .ForMember(dest => dest.SelectedItem, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.LabourCost, opt => opt.MapFrom(src => src.Item!.AssemblyPrice))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))
                .ForMember(dest => dest.IsEditable, opt => opt.MapFrom(src => false)) // default false unless UI overrides

             .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null ? src.MfgPoSub.MfgPo.PONo + "" + src.MfgPoSub.MfgPo.Suffix : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src => src.MfgPoSub != null ? src.MfgPoSub.MfgPo.PODate : (DateTime?)null))

                .ForMember(dest => dest.PoBalQty,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null ? src.MfgPoSub.BalQty : 0))

            .ForMember(dest => dest.LineId,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null ? src.MfgPoSub.LineNo : 0));



            // ================== VM -> Entity ==================
            CreateMap<MfgDcSubVM, MfgDcSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())          // handled by ItemId
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())    // handled by CostId
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())      // handled by RefPoSubId
                .ForMember(dest => dest.MfgDc, opt => opt.Ignore());        // handled by DcId
        }
    }



}
