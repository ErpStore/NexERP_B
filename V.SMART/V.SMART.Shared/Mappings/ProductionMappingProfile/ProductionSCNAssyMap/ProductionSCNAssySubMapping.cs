using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionSCNAssyMap
{
    public class ProductionSCNAssySubMapping : Profile
    {
        public ProductionSCNAssySubMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionSCNAssySub, ProductionSCNAssySubVM>()

                 .ForMember(dest => dest.RefReturnNo,
                    opt => opt.MapFrom(src =>
                        src.ProductionReturnAssySubs != null
                            ? src.ProductionReturnAssySubs.ProductionReturnAssy.ReturnNo + "" + src.ProductionReturnAssySubs.ProductionReturnAssy.Suffix
                            : string.Empty))
                .ForMember(dest => dest.RefReturnDate,
                    opt => opt.MapFrom(src =>
                        src.ProductionReturnAssySubs != null
                            ? src.ProductionReturnAssySubs.ProductionReturnAssy.ReturnDate
                            : (DateTime?)null))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))


                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));


            // ===================== VM -> Entity =====================
            CreateMap<ProductionSCNAssySubVM, ProductionSCNAssySub>()
                .ForMember(dest => dest.ProductionReturnAssySubs, opt => opt.Ignore())
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionSCNAssy, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());


        }
    }
}
