using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionIssueAssyMap
{
    public class ProductionIssAssySubMapping : Profile
    {
        public ProductionIssAssySubMapping()
        {

            // ===================== Entity -> VM =====================
            CreateMap<ProductionIssueAssySub, ProductionIssueAssySubVM>()

                 //.ForMember(dest => dest.JobOrderNo,
                 //    opt => opt.MapFrom(src =>
                 //        src.JobOrder != null
                 //            ? src.JobOrder.JobNo + "" + src.JobOrder.Suffix
                 //            : string.Empty))
                 .ForMember(dest => dest.JobOrderNo,
                                opt => opt.MapFrom(src =>
                                    src.JobOrder == null
                                        ? ""
                                        : (src.JobOrder.StaffID == null
                                            ? $"{src.JobOrder.JobNo}{src.JobOrder.Suffix ?? ""}"
                                            : (!string.IsNullOrWhiteSpace(src.JobOrder.Staff.DepartmentCode)
                                                ? $"{src.JobOrder.Staff.DepartmentCode}/{src.JobOrder.JobNo}{src.JobOrder.Suffix ?? ""}"
                                                : $"{src.JobOrder.JobNo}{src.JobOrder.Suffix ?? ""}"
                                              )
                                          )
                                ))

                .ForMember(dest => dest.JobOrderDate,
                    opt => opt.MapFrom(src =>
                        src.JobOrder != null
                            ? src.JobOrder.JobDate
                            : (DateTime?)null))

                .ForMember(dest => dest.AssyItemCode, opt => opt.MapFrom(src => src.AssyItem != null ? src.AssyItem.ItemCode : string.Empty))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));



            // ===================== VM -> Entity =====================
            CreateMap<ProductionIssueAssySubVM, ProductionIssueAssySub>()
                .ForMember(dest => dest.ProductionIssueAssyParent, opt => opt.Ignore())
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.JobOrderSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());


        }
    }
}
