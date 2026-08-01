using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.EstimationViewModel;
using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IEstimateService
    {
        Task<string> GetEstimateNumberAsync(string suffix);
        Task<int> GetDecimalPlacesAsync();
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<EstimateSubVM>> GetItemProcessList(int itemId, int estimateId);//ItemProcess
    
        Task<List<Process>> GetAllProcessAsync();
        Task<List<ItemVM>> GetAllActiveItemsAsync();//All Items
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText); //3 Letters Item Search
        Task<EstimateVM> Calculate(EstimateVM vm);
        Task<EstimateVM> UpsertEstimateAsync(EstimateVM estimate);
        Task<(List<EstimateVM> EstimateVM, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<EstimateVM?> GetEstimationByEstimateIdAsync(int EstimateId);
        Task<(bool CanDelete, string Message)> CanDeleteEstimateAsync(int EstiamateId);
        Task<bool> DeleteEstimationByEstimateIdAsync(int EstiamateId);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);

        Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId);//bool mfgOrLab

        Task<List<ItemVM>> GetItemByEnquirySubId(int EnquiryId);
        Task<int> GetEnqSubId(int enquiryId, int itemId);

        Task<string?> GetEnquiryNo(int enquirySubId);

        Task<List<EstimateVM>> GetItemDetails(int itemId); //ItemDetails

        Task<decimal> ProcessRate(int SelectedProcessId);
        Task<bool> IsProcessFlowExists(int itemId);

    }
}
