using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockIssueRequestVM;

namespace V.SMART.Shared.Mappings.Inventory_Stock.MaterialIssueProfile
{
    public class StockIssueRequseSubProfile : Profile
    {
        public StockIssueRequseSubProfile()
        {
            // ===================== Entity → ViewModel =====================
            CreateMap<StockIssueRequestSub, StockIssueRequestSubVM>()
                 // Item Details
                 .ForMember(dest => dest.SelectedItemVMs, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                // Map CostCenter ProjectNo (if applicable)
                .ForMember(dest => dest.ProjectNo,
                    opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))
                .ForMember(dest => dest.IsEditable, opt => opt.MapFrom(src => false));


            // ===================== ViewModel → Entity =====================
            CreateMap<StockIssueRequestSubVM, StockIssueRequestSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.StockIssueRequest, opt => opt.Ignore());
        }
    }
}
