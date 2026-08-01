using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MfgInvoice_Profile
{
    public class MfgInvSubProfile:Profile
    {

        public MfgInvSubProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<MfgInvSub, MfgInvSubVM>()

                // PO Details
                .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null && src.MfgPoSub.MfgPo != null
                            ? $"{src.MfgPoSub.MfgPo.PONo}{src.MfgPoSub.MfgPo.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null && src.MfgPoSub.MfgPo != null
                            ? src.MfgPoSub.MfgPo.PODate
                            : (DateTime?)null))

                .ForMember(dest => dest.PoBalQty,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.BalQty
                            : 0))

                // DC Details
                .ForMember(dest => dest.RefDcNo,
                    opt => opt.MapFrom(src =>
                        src.MfgDcSub != null && src.MfgDcSub.MfgDc != null
                            ? $"{src.MfgDcSub.MfgDc.DcNo}{src.MfgDcSub.MfgDc.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.RefDcDate,
                    opt => opt.MapFrom(src =>
                        src.MfgDcSub != null && src.MfgDcSub.MfgDc != null
                            ? src.MfgDcSub.MfgDc.DcDate
                            : (DateTime?)null))

                .ForMember(dest => dest.DcBalQty,
                    opt => opt.MapFrom(src =>
                        src.MfgDcSub != null
                            ? src.MfgDcSub.BalQty
                            : 0))

                // Item Details
                .ForMember(dest => dest.ItemCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))

                .ForMember(dest => dest.ItemName,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))

                .ForMember(dest => dest.HsnCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))

                .ForMember(dest => dest.MeasureUnit,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.Specification,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))

                .ForMember(dest => dest.LabourCost,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.AssemblyPrice : 0))

                .ForMember(dest => dest.Category,
                    opt => opt.MapFrom(src =>
                        src.Item != null && src.Item.Category != null
                            ? src.Item.Category.CategoryName
                            : string.Empty))

                // Credit Notes
                .ForMember(dest => dest.CreditNos,
                    opt => opt.MapFrom(src =>
                        src.CreditNoteSubs
                            .Where(x => x.CreditNote != null)
                            .Select(x => $"{x.CreditNote.CreditNo}{x.CreditNote.Suffix}")
                            .Distinct()
                            .ToList()))

                .ForMember(dest => dest.CreditDates,
                    opt => opt.MapFrom(src =>
                        src.CreditNoteSubs
                            .Where(x => x.CreditNote != null)
                            .Select(x => x.CreditNote.CreditDate)
                            .Distinct()
                            .ToList()))

                // Selected Item
                .ForMember(dest => dest.SelectedItem,
                    opt => opt.MapFrom(src =>
                        src.Item == null
                            ? null
                            : new ItemVM
                            {
                                ItemId = src.Item.ItemId,
                                ItemCode = src.Item.ItemCode,
                                ItemName = src.Item.ItemName,
                                HSNCode = src.Item.HSNCode,
                                MeasureUnit = src.Item.MeasureUnit,
                                Specification = src.Item.Specification,
                                CategoryName = src.Item.Category != null
                                    ? src.Item.Category.CategoryName
                                    : string.Empty
                            }));

            // ===================== VM -> Entity =====================
            CreateMap<MfgInvSubVM, MfgInvSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.MfgDcSub, opt => opt.Ignore())
                .ForMember(dest => dest.MfgInv, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNoteSubs, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        }
        //public MfgInvSubProfile() 
        //{
        //    // ===================== Entity -> VM =====================
        //    CreateMap<MfgInvSub, MfgInvSubVM>()


        //        .ForMember(dest => dest.RefPoNo,
        //            opt => opt.MapFrom(src =>
        //                src.MfgPoSub != null? src.MfgPoSub.MfgPo.PONo + "" + src.MfgPoSub.MfgPo.Suffix: string.Empty))

        //        .ForMember(dest => dest.RefPoDate,
        //            opt => opt.MapFrom(src => src.MfgPoSub != null ? src.MfgPoSub.MfgPo.PODate : (DateTime?)null))

        //        .ForMember(dest => dest.PoBalQty,
        //            opt => opt.MapFrom(src =>
        //                src.MfgPoSub != null ? src.MfgPoSub.BalQty :0))

        //        .ForMember(dest => dest. RefDcNo,
        //            opt => opt.MapFrom(src =>
        //                src.MfgDcSub != null
        //                    ? src.MfgDcSub.MfgDc.DcNo + "" + src.MfgDcSub.MfgDc.Suffix
        //                    : string.Empty))

        //        .ForMember(dest => dest.RefDcDate,
        //            opt => opt.MapFrom(src =>
        //                src.MfgDcSub != null
        //                    ? src.MfgDcSub.MfgDc.DcDate
        //                    : (DateTime?)null))

        //        .ForMember(dest => dest.DcBalQty,
        //            opt => opt.MapFrom(src =>
        //                src.MfgDcSub != null ? src.MfgDcSub.BalQty : 0))

        //        // Item
        //        .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
        //        .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
        //        .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
        //        .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
        //        .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
        //        .ForMember(dest => dest.LabourCost, opt => opt.MapFrom(src => src.Item!.AssemblyPrice))
        //        .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty))

        //        .ForMember(dest => dest.CreditNos,
        //                opt => opt.MapFrom(src =>
        //                    src.CreditNoteSubs
        //                        .Select(x => x.CreditNote.CreditNo + x.CreditNote.Suffix)
        //                        .Distinct()
        //                        .ToList()))

        //            .ForMember(dest => dest.CreditDates,
        //                opt => opt.MapFrom(src =>
        //                    src.CreditNoteSubs
        //                        .Select(x => x.CreditNote.CreditDate)
        //                        .Distinct() .ToList()))

        //        .ForMember(dest => dest.SelectedItem, opt => opt.MapFrom(src => src.Item != null ? new ItemVM
        //        {
        //            ItemId = src.Item.ItemId,
        //            ItemCode = src.Item.ItemCode,
        //            ItemName = src.Item.ItemName,
        //            HSNCode = src.Item.HSNCode,
        //            MeasureUnit = src.Item.MeasureUnit,
        //            Specification = src.Item.Specification,
        //            CategoryName=src.Item.Category.CategoryName
        //        } : null));

        //    // ===================== VM -> Entity =====================
        //    CreateMap<MfgInvSubVM, MfgInvSub>()
        //        .ForMember(dest => dest.Item, opt => opt.Ignore())
        //        .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
        //        .ForMember(dest => dest.MfgDcSub, opt => opt.Ignore())
        //        .ForMember(dest => dest.MfgInv, opt => opt.Ignore())
        //        .ForMember(dest => dest.CreditNoteSubs, opt => opt.Ignore())
        //        .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        //}
    }
}
