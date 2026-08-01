using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.CreditNote_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.CreditNote_Profile
{
    public class CreditNoteSubProfile:Profile
    {
        public CreditNoteSubProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<CreditNoteSub, CreditNoteSubVM>()

                .ForMember(dest => dest.RefInvNo, opt => opt.MapFrom(src =>
                        src.MfgInvSub != null ? src.MfgInvSub.MfgInv.InvNo + "" + src.MfgInvSub.MfgInv.Suffix : string.Empty))
                .ForMember(dest => dest.RefInvDate, opt => opt.MapFrom(src => src.MfgInvSub != null ? src.MfgInvSub.MfgInv.InvDate : (DateTime?)null))


                // Item
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty))

                .ForMember(dest => dest.SelectedItem, opt => opt.MapFrom(src => src.Item != null ? new ItemVM
                {
                    ItemId = src.Item.ItemId,
                    ItemCode = src.Item.ItemCode,
                    ItemName = src.Item.ItemName,
                    HSNCode = src.Item.HSNCode,
                    MeasureUnit = src.Item.MeasureUnit,
                    Specification = src.Item.Specification,
                    CategoryName = src.Item.Category.CategoryName
                } : null));

            // ===================== VM -> Entity =====================
            CreateMap<CreditNoteSubVM, CreditNoteSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNote, opt => opt.Ignore())
                .ForMember(dest => dest.MfgInvSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        }
    }
}
