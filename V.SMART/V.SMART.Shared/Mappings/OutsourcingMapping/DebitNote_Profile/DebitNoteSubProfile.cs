using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.DebitNote_VM;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.DebitNote_Profile
{
    public class DebitNoteSubProfile:Profile
    {
        public DebitNoteSubProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<DebitNoteSub, DebitNoteSubVM>()

                .ForMember(dest => dest.RefPurcInvNo, opt => opt.MapFrom(src =>
                        src.PurchaseInvoiceSub != null ? src.PurchaseInvoiceSub.PurchaseInvoice.InvNo + "" + src.PurchaseInvoiceSub.PurchaseInvoice.Suffix : string.Empty))
                .ForMember(dest => dest.RefPurchInvDate, opt => opt.MapFrom(src => src.PurchaseInvoiceSub != null ? src.PurchaseInvoiceSub.PurchaseInvoice.InvDate : (DateTime?)null))

                .ForMember(dest => dest.RefSubConInvNo, opt => opt.MapFrom(src =>
                        src.SubConInvSub != null ? src.SubConInvSub.SubConInv.InvNo + "" + src.SubConInvSub.SubConInv.Suffix : string.Empty))
                .ForMember(dest => dest.RefSubConInvDate, opt => opt.MapFrom(src => src.SubConInvSub != null ? src.SubConInvSub.SubConInv.InvDate : (DateTime?)null))


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
            CreateMap<DebitNoteSubVM, DebitNoteSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.DebitNote, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseInvoiceSub, opt => opt.Ignore())
                .ForMember(dest => dest.SubConInvSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        }
    }
}
