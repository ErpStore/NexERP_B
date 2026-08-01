using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.BreakdownMaintenanceVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService
{
    public interface IBreakdownMaintenanceService
    {
        Task<bool> FindDuplicateMaintenanceRecords(int machineId, DateTime breakdownDate, int? excludeId = null);
        
        Task<BreakdownMaintenanceVM> GetBreakdownMaintenanceById(int id);
       
        Task CreateBreakdownMaintenance(BreakdownMaintenanceVM breakdownMaintenance);
       
        Task<BreakdownMaintenanceVM> UpdateBreakdownMaintenanceAsync(BreakdownMaintenanceVM breakdownMaintenance);
       
        Task<bool> DeleteBreakdownMaintenance(int id);

        Task<(List<BreakdownMaintenanceVM> BreakdownVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<Decimal> GetMachinePrice(int machineId);
    }
}
