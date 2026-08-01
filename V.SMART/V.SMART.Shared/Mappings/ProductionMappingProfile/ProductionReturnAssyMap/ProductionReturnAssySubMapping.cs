using AutoMapper;
using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionReturnAssyMap
{
    public class ProductionReturnAssySubMapping : Profile
    {
        public ProductionReturnAssySubMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionReturnAssySub, ProductionReturnAssySubVM>()

                //.ForMember(dest => dest.RefJobOrderNo,
                //   opt => opt.MapFrom(src =>
                //       src.JobOrder != null
                //           ? src.JobOrder.JobNo + "" + src.JobOrder.Suffix
                //           : string.Empty))
                .ForMember(dest => dest.RefJobOrderNo,
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


                .ForMember(dest => dest.RefJobOrderDate,
                    opt => opt.MapFrom(src =>
                        src.JobOrder != null
                            ? src.JobOrder.JobDate
                            : (DateTime?)null))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))


                .ForMember(dest => dest.InspectNo,
                    opt => opt.MapFrom(src =>
                        src.IncomingInspection != null
                            ? $"{src.IncomingInspection.InspectNo}{src.IncomingInspection.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.InspectDate,
                    opt => opt.MapFrom(src => src.IncomingInspection != null
                        ? src.IncomingInspection.InspectDate
                        : (DateTime?)null));



            // ===================== VM -> Entity =====================
            CreateMap<ProductionReturnAssySubVM, ProductionReturnAssySub>()
                .ForMember(dest => dest.ProductionReturnAssy, opt => opt.Ignore())
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.JobOrder, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());


        }
    }
}
