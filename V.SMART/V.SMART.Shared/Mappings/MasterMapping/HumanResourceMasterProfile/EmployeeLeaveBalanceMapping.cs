using AutoMapper;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.Mappings.MasterMapping.HumanResourceMasterProfile
{
    public class EmployeeLeaveBalanceMapping : Profile
    {
        public EmployeeLeaveBalanceMapping()
        {
            // Entity -> VM
            CreateMap<EmployeeLeaveBalance, EmployeeLeaveBalanceVM>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Staff != null ? src.Staff.StaffName : string.Empty))
                .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType != null ? src.LeaveType.LeaveName : string.Empty));


            // VM -> Entity
            CreateMap<EmployeeLeaveBalanceVM, EmployeeLeaveBalance>()
                .ForMember(dest => dest.Staff, opt => opt.Ignore())
                .ForMember(dest => dest.LeaveType, opt => opt.Ignore());

        }
    }
}
