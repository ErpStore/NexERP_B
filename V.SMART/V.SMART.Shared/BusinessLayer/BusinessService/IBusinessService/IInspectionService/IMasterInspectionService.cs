using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInspectionService
{
    public interface IMasterInspectionService
    {
        //Common Services for Inspection Module
        Task<List<ItemVM>> GetAllInstrumentsAsync();

        Task<int> GetNoOfSamples();

        Task<List<DefectInfo>> GetAllActiveDefectsAsync();

        Task<List<CostCenterVM>> GetAllCostCenterDetails();

        Task<List<ItemVM>> GetAllComponentsAsync();

        Task<List<Staff>> GetAllStaffAsync();

        Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId, int ProcessId);

        Task<IEnumerable<Process>> GetAllProcessAsync();

        //Developer Services for Master Inspection Module
        Task<MasterInspectionVM?> GetInspectionByIdAsync(int inspectionId);
        Task<MasterInspectionVM> UpsertInspectAsync(MasterInspectionVM quote);
        Task<bool> DeleteInspectionByIdAsync(int inspectionId);

        Task<(List<MasterInspectionVM> FinalInspectionVMs, int TotalCount)>
         SearchFinalInspectionAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);

        Task<string> GenerateNewInspectionNoAsync();
    }
}
