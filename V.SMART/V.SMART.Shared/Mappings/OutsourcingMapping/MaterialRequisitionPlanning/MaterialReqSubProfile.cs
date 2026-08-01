using AutoMapper;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.MaterialRequisitionPlanning
{
    public class MaterialReqSubProfile : Profile
    {
        public MaterialReqSubProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<MaterialReqSub, MaterialReqSubVM>()

                //Customer Mapping  

                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

                .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PONo + "" + src.MfgPoSub.MfgPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PODate
                            : (DateTime?)null))

                // Item Mapping

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit ?? string.Empty : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName ?? string.Empty : string.Empty))

                // Vendor
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName ?? string.Empty : string.Empty))

                // Cost Center
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo ?? string.Empty : string.Empty))

                // Computed fields (ignored for mapping)
                .ForMember(dest => dest.StockQty, opt => opt.Ignore()) // manually filled later
                .ForMember(dest => dest.SelectedItemVM, opt => opt.Ignore())
                .ForMember(dest => dest.TotalGross, opt => opt.MapFrom(src => (src.PurchQty * src.UnitPrice)));

            // ===================== VM -> Entity =====================
            CreateMap<MaterialReqSubVM, MaterialReqSub>()
                // Relationships (handled via IDs)
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.AssmblyDef, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())

                // Skip calculated fields
                .ForMember(dest => dest.TotalGross, opt => opt.Ignore());
        }
    }


}
