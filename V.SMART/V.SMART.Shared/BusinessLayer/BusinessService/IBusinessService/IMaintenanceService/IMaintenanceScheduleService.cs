using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService
{
    public interface IMaintenanceScheduleService
    {
        Task<bool> DeleteMaintenanceScheduleAsync(int scheduleId);

        Task CreateMaintenanceSchedule(MaintenanceScheduleVM maintenanceSchedule);

        Task UpdateMaintenanceScheduleAsync(MaintenanceScheduleVM maintenanceSchedule);

        Task<List<MaintenanceScheduleVM>> GetAllMaintenanceVMsAsync();

        Task<List<MaintenanceScheduleVM>> FindMachineIdById(int MachId);

        Task<MaintenanceScheduleVM>  GetMaintenanceScheduleById(int ScheduleId);

        Task<string> GenerateDocumentNo();
    }
}
