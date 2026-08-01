using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.StaffLoan;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.StaffLoanViewModel;

namespace V.SMART.Shared.Mappings.HumanResource.AttendProfile
{
    public class StaffLoanProfile : Profile
    {
        public StaffLoanProfile()
        {
            // ========================
            // VM → Entity
            // ========================
            CreateMap<StaffLoanVM, StaffLoan>()

                //.ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                //.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                //.ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
                //.ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                .ForMember(dest => dest.LoanType, opt => opt.MapFrom(src => src.LoanType))
                .ForMember(dest => dest.NumberOfMonthsRecoveries, opt => opt.MapFrom(src => src.NumberOfMonthsRecoveries))
                .ForMember(dest => dest.LoanNo, opt => opt.MapFrom(src => src.LoanNo))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                // IMPORTANT: Ignore unmapped properties manually
                .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.LoanId));

            // ========================
            // Entity → VM
            // ========================
            CreateMap<StaffLoan, StaffLoanVM>()

                //.ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                //.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                //.ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
                //.ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                .ForMember(dest => dest.NumberOfMonthsRecoveries, opt => opt.MapFrom(src => src.NumberOfMonthsRecoveries))
                .ForMember(dest => dest.LoanType, opt => opt.MapFrom(src => src.LoanType))
                .ForMember(dest => dest.LoanNo, opt => opt.MapFrom(src => src.LoanNo))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));
        }
    }
}
