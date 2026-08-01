using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.PerformaInvoiceProfile
{
    public class PerformaInvSubProfile : Profile
    {
        public PerformaInvSubProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<PerformaInvSub, PerformaInvSubVM>()


                .ForMember(dest => dest.RefPoNo ,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PONo + "" + src.MfgPoSub.MfgPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PODate
                            : null))

                // Item
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.SelectedItem, opt => opt.MapFrom(src => src.Item != null ? new ItemVM
                {
                    ItemId = src.Item.ItemId,
                    ItemCode = src.Item.ItemCode,
                    ItemName = src.Item.ItemName,
                    HSNCode = src.Item.HSNCode,
                    MeasureUnit = src.Item.MeasureUnit
                } : null));

            // ===================== VM -> Entity =====================
            CreateMap<PerformaInvSubVM, PerformaInvSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.PerformaInv, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        }
    }
}
