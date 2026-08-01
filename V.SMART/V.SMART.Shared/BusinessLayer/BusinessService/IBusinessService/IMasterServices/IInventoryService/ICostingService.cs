using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface ICostingService
    {
        
        Task<List<AssemblyDefLabourVM>> GetAssemblyDefByAssemblyIdAsync(int assemblyId);
        Task UpdateBOMOnlyAsync(AssemblyDefLabourVM parentAssyVM, List<AssemblyDefLabourVM> bomRows,List<AssemblyChargeVM> chargeVMs);

        Task<List<AssemblyChargeVM>> GetAssemblyChargesByAssemblyIdAsync(int assemblyId);
        Task<List<AssemblyDefLabourVM>> GetAssemblyModifyTrackByAssyId(int assyId);

        Task<bool> IsAdditionalChargesEnabledAsync();

        Task<bool> IsLabourChargesDisplayEnabledAsync();

        Task<bool> IsLabourMaterialValidateEnabledAsync();
        Task<Dictionary<int, decimal>> GetBulkLabourCostsAsync(List<int> itemIds);

       

    }
}
