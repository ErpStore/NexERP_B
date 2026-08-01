using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance;
using V.SMART.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService
{
    public interface ICalibrationHistoryAndMaintenanceService
    {
        Task<List<ItemVM>> GetAllInstrumentsAsync();

        Task<(List<CalibrationHistoryVM> CalibrationVMs, int TotalCount)>
        SearchCalibrationHistoriesAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<CalibrationHistoryVM> GetCalbrationById(int id);

        Task<InstrumentDetailsVM> GetInstrumentDetailsById(int id);

        Task UpdatedCancelStatus(CalibrationHistoryVM calibrate);

        Task<List<InstrumentDetailsVM>> FindAllUID(int id);

        Task<bool> IsUniqueIdExists(int ItemId, string uniqueId);

        Task<bool> IsReportNoExists(string calibrateno);

        Task<InstrumentDetailsVM> CreateInstrumentDetailsAsync(InstrumentDetailsVM instrumentDetails);

        Task CreateCalibrationAsync(CalibrationHistoryVM calibrationHistory);

        Task<InstrumentDetailsVM> UpdateInstrumentDetails(InstrumentDetailsVM instrumentDetails);

        Task<bool> DeleteCalibrationHistory(int id);
    }
}
