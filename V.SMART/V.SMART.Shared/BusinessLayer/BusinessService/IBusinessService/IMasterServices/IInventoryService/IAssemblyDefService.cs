using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Company_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IAssemblyDefService
    {
        // 🔹 Item Search Related
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchOnlyAssyItemsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchAssyAndSubAssyItemsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchSubAssyAndItemsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchOnlyComponentItems(string searchText);
        Task<IEnumerable<ItemVM>> SearchBOMAssinedComponentItems(string searchText);
        Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<IEnumerable<ItemVM>> SearchOnlyLabourItems(string searchText);

        Task<Companydetails?> GetCompanyDetailsAsync();


        // 🔹 BOM (Bill of Material) Related

        Task<(List<AssemblyDefVM> assemblyDefVMs, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task UploadBomAsync(List<BomUploadRowViewModel> bomList);
        Task<List<AssmblyDef>> GetAllAsyItemsByAssyId(int assemblyId);
        Task<List<AssemblyDefVM>> GetAllAssembliesAsync();
        Task<List<AssemblyModify>> GetAllAssyModifyTrakByAssyId(int assemblyId);
        Task<AssemblyDefVM> GetAssyByAssemblyIdAsync(int assemblyId);
        Task<IEnumerable<Process>> LoadSelectedProcessAsync(int assyId);
        Task<(decimal UtilQty, decimal BalQty)> GetUtiliyQtyById(int id);
        Task<bool> IsAssyExistsInAssyDefAsync(int assyId);
        Task<List<AssemblyDefVM>> GetAssemblyDefByAssemblyIdAsync(int assemblyId);
        Task DeleteAndResequenceAsync(AssemblyDefVM assemblyDef);
        Task UpdateItemCancelAsync(AssemblyDefVM subItem);
        Task UpsertBOMAsync(bool isNew, AssemblyDefVM parentAssyVM, List<AssemblyDefVM> bomRows);
        Task<bool> IsNewBOMAsync(int assemblyId);
        Task DeleteAssemblyRelatedDataAsync(int assyId, List<AssmblyDef> allAssemblyRows);
        Task DeleteAssemblyModifyTrackRelatedDataAsync(List<AssemblyModify> assyModifyTrack);
        Task<bool> HasAnyBOMTransactionMadeAsync(int AssyId);
        Task<List<AssemblyModifyTrackVM>> GetAssemblyModifyTrackByAssyId(int assyId);
        Task ConvertBOMToPRAsync(int mainAssy, List<AssemblyDefVM> bomRows);
        Task<SCNGenVM> AddStockFromBOM(int assemblyId, int? SubAssyId, int? subAssyId2, int? subAssyid3, List<AssemblyDefVM> bomRows);
        Task<(bool CanDelete, string Message)> CanDeleteAssydefAsync(int id);
        Task<bool> DeleteBOMAsync(int bomId);

        Task<int> GetMaxSlNoByAssemblyIdAsync(int assemblyId);
        Task<AssmblyDef> GetByAssemblyAndItemAsync(int assemblyId, int itemId);
    }
}
