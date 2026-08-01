using V.SMART.Shared.Data.Maintenance.Maintenance_Process;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService
{
    public interface IMaintenanceProcessService
    {
        Task<(bool CanSave, String Message)> IsValid(List<MaintenanceProcess> list);
        Task<List<MaintenanceScheduleVM>> ShowMaintenanceProcessList(DateTime SelectedDate);
        Task<String> GetLatestMaintenanceDate(int MachId);
        Task<DateTime> GetNextDueDate(int MainId, DateTime SelectedDate);
        Task<String> GetMaintenanceNameById(int MainId);
        Task<List<MaintenanceProcess>> GetAllHistory(int MachId);
        Task<List<MaintenanceProcess>> GetMaintenanceProcessesById(int machId, int mainId, int ScheduleId, DateTime selecteddate);
        Task<bool> FindStatus(int machId, int mainId,int ScheduleId, DateTime selecteddate);
        Task UpdateStatus(int machId, int mainId, int ScheduleId, DateTime selecteddate);
        Task<Dictionary<(int MachineId, int ScheduleId, DateTime Date), List<MaintenanceProcess>>> FindBetweenProcess(DateTime? fromdate, DateTime? todate);
        Task<List<MaintenanceScheduleVM>> FindSchedules(DateTime? fromdate, DateTime? todate);
    }
}
