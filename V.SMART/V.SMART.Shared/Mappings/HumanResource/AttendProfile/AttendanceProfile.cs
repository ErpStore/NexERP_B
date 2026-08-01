using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.Attendance;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AttendanceeVM;

namespace V.SMART.Shared.Mappings.HumanResource.AttendProfile
{
    public class AttendanceProfile : Profile
    {
        public AttendanceProfile()
        {
            // ========================
            // VM → Entity
            // ========================
            CreateMap<AttendanceVM, Attendance>()
                .ForMember(dest => dest.Ldetail, opt => opt.Ignore())
                .ForMember(dest => dest.OtDetail, opt => opt.Ignore())
                .ForMember(dest => dest.InTime, opt => opt.Ignore())
                .ForMember(dest => dest.OutTime, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.BioMetricId, opt => opt.MapFrom(src => src.BioMetricId))
                .ForMember(dest => dest.Reporting, opt => opt.MapFrom(src => src.Reporting))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.MonthId, opt => opt.MapFrom(src => src.MonthId))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                // IMPORTANT: Ignore unmapped properties manually
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // ========================
            // Entity → VM
            // ========================
            CreateMap<Attendance, AttendanceVM>()
                .ForMember(dest => dest.Ldetail, opt => opt.MapFrom(src => src.Ldetail))
                .ForMember(dest => dest.OtDetail, opt => opt.MapFrom(src => src.OtDetail))
                .ForMember(dest => dest.InTime, opt => opt.MapFrom(src => src.InTime))
                .ForMember(dest => dest.OutTime, opt => opt.MapFrom(src => src.OutTime))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.BioMetricId, opt => opt.MapFrom(src => src.BioMetricId))
                .ForMember(dest => dest.Reporting, opt => opt.MapFrom(src => src.Reporting))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.MonthId, opt => opt.MapFrom(src => src.MonthId))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId));
        }
    }
}
