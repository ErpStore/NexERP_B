
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.IncomingInspectionVM;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInspectionService
{
    public interface IIncomingInspectionService
    {
        //Common Services for Inspection Module
        Task<List<ItemVM>> GetAllInstrumentsAsync();

        Task<int> GetNoOfSamples();

        Task<List<CustomerVM>> GetAllActiveCustomersAsync();

        Task<List<DefectInfo>> GetAllActiveDefectsAsync();

        Task<List<CostCenterVM>> GetAllCostCenterDetails();

        Task<List<ItemVM>> GetAllComponentsAsync();

        Task<List<Staff>> GetAllStaffAsync();

        Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId, int ProcessId);

        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<List<TermsAndConditions>> GetTermsAsync();

        Task<String> GetCategorynameByItemIdAsync(int? itemId);


        //Developer Defined Service
        Task<string> GenerateNewInspectionNoAsync(String Screen);

        Task<IncomingInspectionVM> UpsertInspectAsync(IncomingInspectionVM inspectVM);

        Task<(List<IncomingInspectionVM> IncomingInspectionVMs, int TotalCount)>
           SearchIncomingInspectionAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<IncomingInspectionVM?> GetInspectionByIdAsync(int inspectionId);

        Task<bool> ValidateSCNgenerated(IncomingInspectionVM data);

        Task<bool> DeleteInspectionByIdAsync(int inspectionId);

        Task<bool> CheckDuplicateRandom(string inspNo, bool isRandom, string suffix);

        Task<List<IncomingInspectionVM>> GetBalQtyDcs();

        Task<List<ProductionLogVM>> GetAllProductionLogsAsync();

        Task<List<ProductionReturnCompVM>> GetAllProductionCompGRNAsync();

        Task<List<ProductionReturnAssyVM>> GetAllProductionAssymblyGRNAsync();

        Task<List<PurchaseGRNVM>> GetAllPurchaseGRNsAsync();

        Task<List<IncomingInspectionVM>> UpdateStatus(List<IncomingInspectionVM> incomingInspectionVMs);
    }
}
