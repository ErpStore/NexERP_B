using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.Inventory_Stock.StockAddProfile
{
    public class StockIssueProfile:Profile
    {
        public StockIssueProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<StockIssue, StockIssueVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item.ItemCode != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.ItemName != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item.Specification != null ? src.Item.Specification : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item.MeasureUnit != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item.HSNCode != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Item.Weight != null ? src.Item.Weight : 0))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item.Category.CategoryName != null ? src.Item.Category.CategoryName : string.Empty))

                // Add Stores
                .ForMember(dest => dest.IssueStoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                .ForMember(dest => dest.ScreenName, opt => opt.MapFrom(src => src.Screen != null ? src.Screen.ScreenName : string.Empty));


            // ===================== VM -> Entity =====================
            CreateMap<StockIssueVM, StockIssue>()
                .ForMember(dest => dest.Item, opt => opt.Ignore()) // handled separately
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.Screen, opt => opt.Ignore());

        }

    }
}
