using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.FinalInspectionVM;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInspectionService
{
    public interface IFinalInspectionService
    {
        //common Services for Inspection Module
        Task<List<ItemVM>> GetAllInstrumentsAsync();

        Task<int> GetNoOfSamples();

        Task<List<CustomerVM>> GetAllActiveCustomersAsync();

        Task<List<DefectInfo>> GetAllActiveDefectsAsync();

        Task<List<CostCenterVM>> GetAllCostCenterDetails();

         Task<IEnumerable<MfgDcVM>> GetAllDcDetailsAsync();

         Task<List<ItemVM>> GetAllComponentsAsync();

         Task<List<Staff>> GetAllStaffAsync();

        Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId);




        //Developer Services for Final Inspection Module
        Task<string> GenerateNewInspectionNoAsync(String Screen);

        Task<FinalInspectionVM> UpsertInspectAsync(FinalInspectionVM inspectVM);

        Task<FinalInspectionVM> GetInspectionByIdAsync(int inspectionId);

        Task<bool> DeleteInspectionByIdAsync(int inspectionId);

        Task<bool> CheckDuplicateRandom(string inspNo,String Screen, string suffix);


        Task<(List<FinalInspectionVM> FinalInspectionVMs, int TotalCount)>
         SearchFinalInspectionAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<FinalInspectionVM> ClearInspection(FinalInspectionVM FinalInspectionVM);
    }
}
